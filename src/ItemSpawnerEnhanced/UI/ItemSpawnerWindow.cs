using System;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using BepInEx.Logging;
using HarmonyLib;
using ItemSpawnerEnhanced.Core;
using ItemSpawnerEnhanced.Localization;
using UnityEngine;
using UnityEngine.UI;

namespace ItemSpawnerEnhanced.UI;

internal sealed class ItemSpawnerWindow : MenuWindow
{
    private static readonly MethodInfo OpenMethod = AccessTools.Method(typeof(MenuWindow), "Open");
    private static readonly MethodInfo CloseMethod = AccessTools.Method(typeof(MenuWindow), "Close");

    private ManualLogSource _logger = null!;
    private ModConfig _settings = null!;
    private LocalizationCatalog _localization = null!;
    private PlayerTargetController _targetController = null!;
    private ItemSpawnerView _view = null!;
    private ItemBrowserController _browser = null!;
    private Coroutine? _rebuildRoutine;
    private Coroutine? _warmUpRoutine;
    private bool _shutdown;

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
        ItemBrowserSession browserSession)
    {
        ItemSpawnerView view = ItemSpawnerView.Create();
        ItemSpawnerWindow? window = null;
        try
        {
            window = view.Root.AddComponent<ItemSpawnerWindow>();
            window.Configure(logger, settings, favorites, browserSession, view);
            return window;
        }
        catch
        {
            window?.Shutdown();
            view.Root.SetActive(false);
            UnityEngine.Object.Destroy(view.Root);
            throw;
        }
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
        StopBackgroundWarmUp();
        StopRebuild();
        LocalizedText.OnLangugageChanged -= OnLanguageChanged;
        if (_settings != null)
            _settings.TagMatchModeEntry.SettingChanged -= OnTagMatchModeChanged;
    }

    protected override void Start()
    {
        base.Start();
        if (_shutdown)
            return;

        _view.SetVisible(false);
        panel.SetActive(true);
        _warmUpRoutine = StartCoroutine(WarmUpInBackground());
    }

    protected override void Update()
    {
        base.Update();
        if (!isOpen)
            return;

        if (_browser.AliasProvidersChanged)
            _browser.RequestRefresh(RefreshRequirement.SearchIndex);

        if (_browser.IsRebuilding)
            return;

        if (_browser.HasPendingRefresh && _browser.CanAttemptRebuild(Time.unscaledTime))
        {
            BeginRequiredRebuild();
            return;
        }

        if (!_targetController.IsRefreshDue(Time.unscaledTime))
            return;

        if (!_browser.IsCatalogCurrent())
        {
            _browser.RequestRefresh(RefreshRequirement.Catalog);
            BeginRequiredRebuild();
        }
        else if (_targetController.Refresh(force: false))
        {
            _browser.RefreshStatus();
        }
    }

    protected override void OnOpen()
    {
        StopBackgroundWarmUp();
        _view.SetVisible(true);
        base.OnOpen();
        bool catalogCurrent = _browser.HasPendingCatalogRefresh || _browser.IsCatalogCurrent();
        if (!_browser.HasPendingCatalogRefresh && !catalogCurrent)
            _browser.RequestRefresh(RefreshRequirement.Catalog);

        if (_browser.AliasProvidersChanged)
            _browser.RequestRefresh(RefreshRequirement.SearchIndex);

        if (_browser.HasPendingRefresh)
        {
            BeginRequiredRebuild();
        }
        else
        {
            _targetController.Refresh(force: true);
            _browser.ApplySearch(_view.SearchText);
        }
        _view.ActivateSearch();
    }

    protected override void OnClose()
    {
        StopRebuild();
        _view.DeactivateSearch();
        _view.SetVisible(false);
        base.OnClose();
    }

    private void Configure(
        ManualLogSource logger,
        ModConfig settings,
        FavoriteStore favorites,
        ItemBrowserSession browserSession,
        ItemSpawnerView view)
    {
        _logger = logger;
        _settings = settings;
        _view = view;
        _localization = new LocalizationCatalog(Assembly.GetExecutingAssembly());
        _targetController = new PlayerTargetController(new PlayerTargetService(logger), view, Localize);
        _browser = new ItemBrowserController(
            logger,
            settings,
            favorites,
            browserSession,
            view,
            _targetController,
            Localize);
        _view.Bind(ToggleWindow, _browser.ApplySearch, OnTargetChanged, _browser.SetTag, _browser.ClearTags);
        _browser.InitializeView();
        ApplyLocalizedChrome();
        LocalizedText.OnLangugageChanged += OnLanguageChanged;
        _settings.TagMatchModeEntry.SettingChanged += OnTagMatchModeChanged;
    }

    private void BeginRequiredRebuild()
    {
        if (_rebuildRoutine != null || _browser.IsRebuilding || !_browser.HasPendingRefresh)
            return;

        _rebuildRoutine = StartCoroutine(RunRequiredRebuild());
    }

    private IEnumerator WarmUpInBackground()
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        yield return null;
        yield return _browser.RebuildRequiredIncrementally();
        _warmUpRoutine = null;
        stopwatch.Stop();
        _logger.LogInfo(
            $"[Perf:UIWarmUp] Full background warm-up completed in {stopwatch.Elapsed.TotalMilliseconds:F2} ms; " +
            $"items={_view.VisibleItemCount}.");
        panel.SetActive(false);
    }

    private void StopBackgroundWarmUp()
    {
        if (_warmUpRoutine == null)
            return;

        StopCoroutine(_warmUpRoutine);
        _warmUpRoutine = null;
        if (_browser != null)
            _browser.AbortRebuild();
    }

    private IEnumerator RunRequiredRebuild()
    {
        yield return null;
        yield return _browser.RebuildRequiredIncrementally();
        _rebuildRoutine = null;
    }

    private void StopRebuild()
    {
        if (_rebuildRoutine != null)
            StopCoroutine(_rebuildRoutine);
        _rebuildRoutine = null;
        if (_browser != null)
            _browser.AbortRebuild();
    }

    private void OnTargetChanged(int index)
    {
        if (_targetController.Select(index))
            _browser.RefreshStatus();
    }

    private void OnTagMatchModeChanged(object? sender, EventArgs eventArgs)
    {
        if (isOpen)
            _browser.ApplySearch(_view.SearchText);
    }

    private void OnLanguageChanged()
    {
        _browser.RequestRefresh(RefreshRequirement.Catalog);
        _targetController.InvalidateLabels();
        ApplyLocalizedChrome();
        if (_warmUpRoutine != null)
        {
            StopBackgroundWarmUp();
            _warmUpRoutine = StartCoroutine(WarmUpInBackground());
            return;
        }

        if (isOpen)
        {
            StopRebuild();
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

    private void OnDestroy()
    {
        Shutdown();
        MenuWindow.AllActiveWindows.Remove(this);
    }
}
