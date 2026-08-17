using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx.Logging;
using HarmonyLib;
using ItemSpawnerEnhanced.Api;
using ItemSpawnerEnhanced.Core;
using ItemSpawnerEnhanced.Localization;
using UnityEngine;
using UnityEngine.UI;

namespace ItemSpawnerEnhanced.UI;

internal sealed class ItemSpawnerWindow : MenuWindow
{
    private const int ItemBatchSize = 24;
    private const float ItemBatchIntervalSeconds = 0.2f;
    private const int SearchIndexBatchSize = 12;
    private static readonly MethodInfo OpenMethod = AccessTools.Method(typeof(MenuWindow), "Open");
    private static readonly MethodInfo CloseMethod = AccessTools.Method(typeof(MenuWindow), "Close");

    private ManualLogSource _logger = null!;
    private ModConfig _settings = null!;
    private FavoriteStore _favorites = null!;
    private FilterSession _filterSession = null!;
    private LocalizationCatalog _localization = null!;
    private GameItemCatalog _catalog = null!;
    private PlayerTargetController _targetController = null!;
    private ItemSpawnerView _view = null!;
    private readonly RefreshState _refresh = new();
    private int _aliasProviderVersion;
    private Coroutine? _rebuildRoutine;
    private float _nextRebuildAttempt;
    private bool _shutdown;

    private bool IsRebuilding => _refresh.IsRebuilding;

    public override bool openOnStart => false;
    public override bool selectOnOpen => true;
    public override Selectable objectToSelectOnOpen => _view.SearchSelectable;
    public override bool closeOnPause => true;
    public override bool closeOnUICancel => true;
    public override bool blocksPlayerInput => true;
    public override bool showCursorWhileOpen => true;
    public override bool autoHideOnClose => true;
    public override GameObject panel => gameObject;

    public static ItemSpawnerWindow Create(
        ManualLogSource logger,
        ModConfig settings,
        FavoriteStore favorites,
        FilterSession filterSession)
    {
        ItemSpawnerView view = ItemSpawnerView.Create();
        ItemSpawnerWindow window = view.Root.AddComponent<ItemSpawnerWindow>();
        window.Configure(logger, settings, favorites, filterSession, view);
        return window;
    }

    public void ToggleWindow()
    {
        try
        {
            (isOpen ? CloseMethod : OpenMethod).Invoke(this, null);
        }
        catch (Exception exception)
        {
            _logger.LogError($"Failed to toggle item spawner window: {exception}");
        }
    }

    public void Shutdown()
    {
        if (_shutdown)
            return;
        _shutdown = true;
        if (_rebuildRoutine != null)
            StopCoroutine(_rebuildRoutine);
        _rebuildRoutine = null;
        _refresh.Abort();
        LocalizedText.OnLangugageChanged -= OnLanguageChanged;
        if (_settings != null)
            _settings.TagMatchModeEntry.SettingChanged -= OnTagMatchModeChanged;
    }

    protected override void Update()
    {
        base.Update();
        if (!isOpen)
            return;

        if (_aliasProviderVersion != SearchAliasRegistry.Version)
            RequestRefresh(RefreshRequirement.SearchIndex);

        if (IsRebuilding)
            return;

        if (_refresh.Pending != RefreshRequirement.None && Time.unscaledTime >= _nextRebuildAttempt)
        {
            BeginRequiredRebuild();
            return;
        }

        if (_targetController.IsRefreshDue(Time.unscaledTime))
        {
            if (!_catalog.IsCurrent())
            {
                RequestRefresh(RefreshRequirement.Catalog);
                BeginRequiredRebuild();
            }
            else
            {
                if (_targetController.Refresh(force: false))
                    RefreshStatus();
            }
        }
    }

    protected override void OnOpen()
    {
        base.OnOpen();
        if (!_catalog.IsCurrent())
            RequestRefresh(RefreshRequirement.Catalog);
        if (_aliasProviderVersion != SearchAliasRegistry.Version)
            RequestRefresh(RefreshRequirement.SearchIndex);
        if (_refresh.Pending != RefreshRequirement.None)
            BeginRequiredRebuild();
        else
        {
            _targetController.Refresh(force: true);
            ApplySearch(_view.SearchText);
        }
        _view.ActivateSearch();
    }

    protected override void OnClose()
    {
        if (_rebuildRoutine != null)
        {
            StopCoroutine(_rebuildRoutine);
            _rebuildRoutine = null;
            _refresh.Abort();
        }
        _view.DeactivateSearch();
        base.OnClose();
    }

    private void Configure(
        ManualLogSource logger,
        ModConfig settings,
        FavoriteStore favorites,
        FilterSession filterSession,
        ItemSpawnerView view)
    {
        _logger = logger;
        _settings = settings;
        _favorites = favorites;
        _filterSession = filterSession;
        _view = view;
        _localization = new LocalizationCatalog(Assembly.GetExecutingAssembly());
        _catalog = new GameItemCatalog(logger, () => settings.ShowAllItems);
        _targetController = new PlayerTargetController(new PlayerTargetService(logger), view, Localize);
        _view.Bind(ToggleWindow, ApplySearch, OnTargetChanged, OnTagChanged, ClearTags);
        _view.SetSelectedTags(_filterSession.SelectedTags);
        LocalizedText.OnLangugageChanged += OnLanguageChanged;
        _settings.TagMatchModeEntry.SettingChanged += OnTagMatchModeChanged;
        ApplyLocalizedChrome();
    }

    private void RequestRefresh(RefreshRequirement requirement)
    {
        _refresh.Request(requirement);
    }

    private void BeginRequiredRebuild()
    {
        if (IsRebuilding || _refresh.Pending == RefreshRequirement.None)
            return;

        if (_rebuildRoutine != null)
            StopCoroutine(_rebuildRoutine);
        _view.SetFavoriteEnabled(false);
        _rebuildRoutine = StartCoroutine(RebuildRequiredIncrementally());
    }

    private IEnumerator RebuildRequiredIncrementally()
    {
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
        _rebuildRoutine = null;
        _refresh.Finish();
        _nextRebuildAttempt = failed ? Time.unscaledTime + 2f : 0f;
        _view.SetFavoriteEnabled(!failed);
        if (failed)
            _view.SetStatus("...", isError: true);
    }

    private void ApplySearch(string query)
    {
        if (IsRebuilding || (_refresh.Pending & RefreshRequirement.SearchIndex) != 0)
            return;

        IReadOnlyList<GameItemRecord> results = _catalog.Search(query)
            .Where(record => ItemFilterMatcher.Matches(
                record.Tags,
                _favorites.IsFavorite(record.Item.name),
                _filterSession.SelectedTags,
                _settings.TagMatchMode))
            .ToArray();
        _view.ShowSearchResults(results);
        RefreshStatus();
    }

    private void OnTargetChanged(int index)
    {
        if (_targetController.Select(index))
            RefreshStatus();
    }

    private void OnTagChanged(ItemFilterTag tag, bool selected)
    {
        if (selected)
            _filterSession.SelectedTags |= tag;
        else
            _filterSession.SelectedTags &= ~tag;
        ApplySearch(_view.SearchText);
    }

    private void ClearTags()
    {
        if (_filterSession.SelectedTags == ItemFilterTag.None)
            return;

        _filterSession.SelectedTags = ItemFilterTag.None;
        _view.SetSelectedTags(ItemFilterTag.None);
        ApplySearch(_view.SearchText);
    }

    private void OnTagMatchModeChanged(object? sender, EventArgs eventArgs)
    {
        if (isOpen)
            ApplySearch(_view.SearchText);
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

    private void RefreshStatus()
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

    private void Spawn(GameItemRecord record)
    {
        if (_targetController.TrySpawn(record.Item, out string errorKey))
            SetStatus(string.Empty, isError: false);
        else
            SetStatus(errorKey, isError: true);
    }

    private void OnLanguageChanged()
    {
        RequestRefresh(RefreshRequirement.Catalog);
        _targetController.InvalidateLabels();
        ApplyLocalizedChrome();
        if (isOpen)
        {
            if (_rebuildRoutine != null)
            {
                StopCoroutine(_rebuildRoutine);
                _rebuildRoutine = null;
                _refresh.Abort();
            }
            BeginRequiredRebuild();
        }
    }

    private void ApplyLocalizedChrome() =>
        _view.SetLocalizedChrome(
            Localize("title"),
            Localize("searchPlaceholder"),
            Localize("clearTagsTooltip"),
            tag => Localize(tag switch
            {
                ItemFilterTag.Favorite => "tagFavorite",
                ItemFilterTag.Food => "tagFood",
                ItemFilterTag.Consumable => "tagConsumable",
                ItemFilterTag.Equipment => "tagEquipment",
                ItemFilterTag.Deployable => "tagDeployable",
                ItemFilterTag.Mystical => "tagMystical",
                ItemFilterTag.Other => "tagOther",
                _ => throw new ArgumentOutOfRangeException(nameof(tag), tag, null)
            }));

    private string Localize(string key) => _localization.Get(GameLanguage.CurrentCode, key);

    private void SetStatus(string key, bool isError)
    {
        _view.SetStatus(string.IsNullOrEmpty(key) ? string.Empty : Localize(key), isError);
    }

    private void OnDestroy()
    {
        Shutdown();
        MenuWindow.AllActiveWindows.Remove(this);
    }
}
