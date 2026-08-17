using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;
using ItemSpawnerEnhanced.Api;
using ItemSpawnerEnhanced.Core;
using UnityEngine;

namespace ItemSpawnerEnhanced.UI;

internal sealed class ItemBrowserController
{
    private const int ItemBatchSize = 24;
    private const float ItemBatchIntervalSeconds = 0.2f;
    private const int SearchIndexBatchSize = 12;

    private readonly ManualLogSource _logger;
    private readonly ModConfig _settings;
    private readonly FavoriteStore _favorites;
    private readonly ItemBrowserSession _session;
    private readonly ItemSpawnerView _view;
    private readonly PlayerTargetController _targetController;
    private readonly Func<string, string> _localize;
    private readonly GameItemCatalog _catalog;
    private readonly RefreshState _refresh = new();
    private int _aliasProviderVersion;
    private float _nextRebuildAttempt;

    public ItemBrowserController(
        ManualLogSource logger,
        ModConfig settings,
        FavoriteStore favorites,
        ItemBrowserSession session,
        ItemSpawnerView view,
        PlayerTargetController targetController,
        Func<string, string> localize)
    {
        _logger = logger;
        _settings = settings;
        _favorites = favorites;
        _session = session;
        _view = view;
        _targetController = targetController;
        _localize = localize;
        _catalog = new GameItemCatalog(logger, () => settings.ShowAllItems);
    }

    public bool IsRebuilding => _refresh.IsRebuilding;
    public bool HasPendingRefresh => _refresh.Pending != RefreshRequirement.None;
    public bool AliasProvidersChanged => _aliasProviderVersion != SearchAliasRegistry.Version;

    public bool CanAttemptRebuild(float currentTime) => currentTime >= _nextRebuildAttempt;

    public bool IsCatalogCurrent() => _catalog.IsCurrent();

    public void InitializeView() => _view.SetSelectedTags(_session.SelectedTags);

    public void RequestRefresh(RefreshRequirement requirement) => _refresh.Request(requirement);

    public void AbortRebuild() => _refresh.Abort();

    public IEnumerator RebuildRequiredIncrementally()
    {
        _view.SetFavoriteEnabled(false);
        _view.SetStatus("...", isError: false);

        if ((_refresh.Pending & RefreshRequirement.Catalog) != 0)
        {
            _refresh.Begin(RebuildPhase.Catalog);
            yield return RebuildCatalogIncrementally();
            if ((_refresh.Pending & RefreshRequirement.Catalog) != 0)
            {
                CompleteRebuild(failed: true);
                yield break;
            }

            yield return null;
            if (_targetController.Refresh(force: true))
                RefreshStatus();
        }

        if ((_refresh.Pending & RefreshRequirement.SearchIndex) != 0)
        {
            _refresh.Begin(RebuildPhase.SearchIndex);
            yield return RebuildSearchIndexIncrementally();
            if ((_refresh.Pending & RefreshRequirement.SearchIndex) != 0)
            {
                CompleteRebuild(failed: true);
                yield break;
            }
        }

        CompleteRebuild(failed: false);
        ApplySearch(_view.SearchText);
    }

    public void ApplySearch(string query)
    {
        if (IsRebuilding || (_refresh.Pending & RefreshRequirement.SearchIndex) != 0)
            return;

        IReadOnlyList<GameItemRecord> results = _catalog.Search(query)
            .Where(record => ItemFilterMatcher.Matches(
                record.Tags,
                _favorites.IsFavorite(record.Item.name),
                _session.SelectedTags,
                _settings.TagMatchMode))
            .ToArray();
        _view.ShowSearchResults(results);
        RefreshStatus();
    }

    public void SetTag(ItemFilterTag tag, bool selected)
    {
        if (selected)
            _session.SelectedTags |= tag;
        else
            _session.SelectedTags &= ~tag;
        ApplySearch(_view.SearchText);
    }

    public void ClearTags()
    {
        if (_session.SelectedTags == ItemFilterTag.None)
            return;

        _session.SelectedTags = ItemFilterTag.None;
        _view.SetSelectedTags(ItemFilterTag.None);
        ApplySearch(_view.SearchText);
    }

    public void RefreshStatus()
    {
        if (IsRebuilding)
        {
            _view.SetStatus("...", isError: false);
            return;
        }

        if (!_targetController.CanSpawn && _view.VisibleItemCount > 0)
        {
            SetStatus(_targetController.ErrorKey, isError: true);
            return;
        }

        SetStatus(_view.VisibleItemCount == 0 ? "noItems" : string.Empty, isError: false);
    }

    private IEnumerator RebuildCatalogIncrementally()
    {
        _view.ClearItems();

        try
        {
            _catalog.RebuildItems();
        }
        catch (Exception exception)
        {
            _logger.LogError($"Failed to rebuild the item catalog: {exception}");
            yield break;
        }

        for (int index = 0; index < _catalog.Items.Count; index++)
        {
            GameItemRecord record = _catalog.Items[index];
            GameItemRecord captured = record;
            try
            {
                _view.AddItem(
                    record,
                    () => Spawn(captured),
                    () => ToggleFavorite(captured),
                    _favorites.IsFavorite(record.Item.name));
            }
            catch (Exception exception)
            {
                _logger.LogError($"Failed to create the item tile for '{record.Item.name}': {exception}");
            }

            if ((index + 1) % ItemBatchSize == 0 && index + 1 < _catalog.Items.Count)
                yield return new WaitForSecondsRealtime(ItemBatchIntervalSeconds);
        }

        _refresh.Complete(RebuildPhase.Catalog);
    }

    private IEnumerator RebuildSearchIndexIncrementally()
    {
        int providerVersion = SearchAliasRegistry.Version;
        IEnumerator rebuild = _catalog.RebuildSearchIndexIncrementally(SearchIndexBatchSize);
        while (true)
        {
            bool hasMore;
            try
            {
                hasMore = rebuild.MoveNext();
            }
            catch (Exception exception)
            {
                _logger.LogError($"Failed to build the item search index: {exception}");
                yield break;
            }

            if (!hasMore)
                break;
            yield return rebuild.Current;
        }

        _refresh.Complete(RebuildPhase.SearchIndex);
        _aliasProviderVersion = providerVersion;
    }

    private void CompleteRebuild(bool failed)
    {
        _refresh.Finish();
        _nextRebuildAttempt = failed ? Time.unscaledTime + 2f : 0f;
        _view.SetFavoriteEnabled(!failed);
        if (failed)
            _view.SetStatus("...", isError: true);
    }

    private void ToggleFavorite(GameItemRecord record)
    {
        if (!_favorites.TryToggle(record.Item.name, out bool isFavorite))
        {
            SetStatus("favoriteSaveFailed", isError: true);
            return;
        }

        _view.SetFavorite(record, isFavorite);
        ApplySearch(_view.SearchText);
    }

    private void Spawn(GameItemRecord record)
    {
        if (_targetController.TrySpawn(record.Item, out string errorKey))
            SetStatus(string.Empty, isError: false);
        else
            SetStatus(errorKey, isError: true);
    }

    private void SetStatus(string key, bool isError) =>
        _view.SetStatus(string.IsNullOrEmpty(key) ? string.Empty : _localize(key), isError);
}
