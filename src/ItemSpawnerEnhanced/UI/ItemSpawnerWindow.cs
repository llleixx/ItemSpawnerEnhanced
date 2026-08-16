using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx.Logging;
using HarmonyLib;
using ItemSpawnerEnhanced.Api;
using ItemSpawnerEnhanced.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ItemSpawnerEnhanced.UI;

internal sealed class ItemSpawnerWindow : MenuWindow
{
    private const int CatalogBatchSize = 12;
    private const int TileBatchSize = 12;
    private static readonly MethodInfo OpenMethod = AccessTools.Method(typeof(MenuWindow), "Open");
    private static readonly MethodInfo CloseMethod = AccessTools.Method(typeof(MenuWindow), "Close");

    private readonly List<ItemTile> _tiles = new();
    private readonly List<PlayerTarget> _dropdownTargets = new();
    private ManualLogSource _logger = null!;
    private LocalizationCatalog _localization = null!;
    private GameItemCatalog _catalog = null!;
    private PlayerTargetService _targets = null!;
    private TMP_FontAsset _font = null!;
    private TMP_InputField _search = null!;
    private TMP_Dropdown _targetDropdown = null!;
    private TextMeshProUGUI _title = null!;
    private TextMeshProUGUI _status = null!;
    private RectTransform _itemContent = null!;
    private int? _manualActorId;
    private bool _catalogDirty = true;
    private Coroutine? _rebuildRoutine;
    private bool _isRebuilding;
    private bool _updatingDropdown;
    private float _lastTargetRefresh;
    private string _targetSignature = string.Empty;
    private bool _shutdown;

    public override bool openOnStart => false;
    public override bool selectOnOpen => true;
    public override Selectable objectToSelectOnOpen => _search;
    public override bool closeOnPause => true;
    public override bool closeOnUICancel => true;
    public override bool blocksPlayerInput => true;
    public override bool showCursorWhileOpen => true;
    public override bool autoHideOnClose => true;
    public override GameObject panel => gameObject;

    public static ItemSpawnerWindow Create(Transform _, ManualLogSource logger)
    {
        var root = new GameObject(
            "ItemSpawnerEnhancedUI",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster));
        root.transform.SetParent(null, false);
        RectTransform rootRect = root.GetComponent<RectTransform>();
        RuntimeUiFactory.Stretch(rootRect);

        Canvas canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 250;
        CanvasScaler scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        TMP_FontAsset font = ResolveFont();
        BuildView(rootRect, font, out ViewReferences view);

        ItemSpawnerWindow window = root.AddComponent<ItemSpawnerWindow>();
        window.Configure(logger, font, view);
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
        _isRebuilding = false;
        LocalizedText.OnLangugageChanged -= OnLanguageChanged;
        SearchAliasRegistry.Changed -= OnAliasProvidersChanged;
    }

    protected override void Update()
    {
        base.Update();
        if (isOpen && Time.unscaledTime - _lastTargetRefresh >= 1f)
            RefreshTargets(force: false);
    }

    protected override void OnOpen()
    {
        base.OnOpen();
        if (!_catalog.IsCurrent())
            _catalogDirty = true;
        if (_catalogDirty)
            BeginCatalogRebuild();
        RefreshTargets(force: true);
        if (!_isRebuilding)
            ApplySearch(_search.text);
        _search.ActivateInputField();
    }

    protected override void OnClose()
    {
        _search.DeactivateInputField();
        base.OnClose();
    }

    private void Configure(ManualLogSource logger, TMP_FontAsset font, ViewReferences view)
    {
        _logger = logger;
        _font = font;
        _localization = new LocalizationCatalog(Assembly.GetExecutingAssembly());
        _catalog = new GameItemCatalog(logger);
        _targets = new PlayerTargetService(logger);
        _search = view.Search;
        _targetDropdown = view.TargetDropdown;
        _title = view.Title;
        _status = view.Status;
        _itemContent = view.ItemContent;

        view.Close.onClick.AddListener(ToggleWindow);
        _search.onValueChanged.AddListener(ApplySearch);
        _targetDropdown.onValueChanged.AddListener(OnTargetChanged);
        LocalizedText.OnLangugageChanged += OnLanguageChanged;
        SearchAliasRegistry.Changed += OnAliasProvidersChanged;
        ApplyLocalizedChrome();
    }

    private void BeginCatalogRebuild()
    {
        if (_rebuildRoutine != null)
            StopCoroutine(_rebuildRoutine);
        _isRebuilding = true;
        _rebuildRoutine = StartCoroutine(RebuildCatalogIncrementally());
    }

    private IEnumerator RebuildCatalogIncrementally()
    {
        _catalogDirty = true;
        _status.text = "...";
        _status.color = RuntimeUiFactory.TextMuted;
        foreach (ItemTile tile in _tiles)
        {
            tile.GameObject.SetActive(false);
            Destroy(tile.GameObject);
        }
        _tiles.Clear();

        IEnumerator rebuild = _catalog.RebuildIncrementally(CatalogBatchSize);
        while (true)
        {
            bool hasMore;
            try
            {
                hasMore = rebuild.MoveNext();
            }
            catch (Exception exception)
            {
                _logger.LogError($"Failed to rebuild the item catalog: {exception}");
                _rebuildRoutine = null;
                _isRebuilding = false;
                yield break;
            }

            if (!hasMore)
                break;
            yield return rebuild.Current;
        }

        for (int index = 0; index < _catalog.Items.Count; index++)
        {
            GameItemRecord record = _catalog.Items[index];
            GameItemRecord captured = record;
            try
            {
                ItemTile tile = RuntimeUiFactory.CreateItemTile(
                    _itemContent,
                    _font,
                    record,
                    () => Spawn(captured));
                tile.Button.interactable = false;
                _tiles.Add(tile);
            }
            catch (Exception exception)
            {
                _logger.LogError($"Failed to create the item tile for '{record.Item.name}': {exception}");
            }

            if ((index + 1) % TileBatchSize == 0 && index + 1 < _catalog.Items.Count)
                yield return null;
        }

        _catalogDirty = false;
        _rebuildRoutine = null;
        _isRebuilding = false;
        Canvas.ForceUpdateCanvases();
        RefreshTargets(force: true);
        ApplySearch(_search.text);
    }

    private void ApplySearch(string query)
    {
        if (_isRebuilding)
            return;

        IReadOnlyList<GameItemRecord> results = _catalog.Search(query);
        foreach (ItemTile tile in _tiles)
            tile.GameObject.SetActive(false);

        var tileMap = _tiles.ToDictionary(tile => tile.Record);
        for (int index = 0; index < results.Count; index++)
        {
            if (!tileMap.TryGetValue(results[index], out ItemTile? tile))
                continue;
            tile.GameObject.SetActive(true);
            tile.GameObject.transform.SetSiblingIndex(index);
        }

        SetStatus(results.Count == 0 ? "noItems" : string.Empty, isError: false);
    }

    private void RefreshTargets(bool force)
    {
        _lastTargetRefresh = Time.unscaledTime;
        IReadOnlyList<PlayerTarget> current = _targets.GetTargets();
        string signature = string.Join("|", current.Select(target =>
            $"{target.ActorId}:{target.Name}:{target.IsLocal}:{target.IsSpectated}:{target.IsValid}"));
        if (!force && signature == _targetSignature)
        {
            RefreshButtonState();
            return;
        }

        _targetSignature = signature;
        _dropdownTargets.Clear();
        _dropdownTargets.AddRange(current.Where(target => target.IsValid));
        if (_manualActorId.HasValue && _dropdownTargets.All(target => target.ActorId != _manualActorId.Value))
            _manualActorId = null;

        Dictionary<string, int> duplicateNames = _dropdownTargets
            .GroupBy(target => target.Name, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.Count(), StringComparer.OrdinalIgnoreCase);

        var options = new List<TMP_Dropdown.OptionData>
        {
            new(Localize("smartTarget"))
        };
        options.AddRange(_dropdownTargets.Select(target =>
        {
            string name = duplicateNames[target.Name] > 1 ? $"{target.Name} #{target.ActorId}" : target.Name;
            if (target.IsLocal)
                name += $" {Localize("youSuffix")}";
            return new TMP_Dropdown.OptionData(name);
        }));

        _updatingDropdown = true;
        _targetDropdown.ClearOptions();
        _targetDropdown.AddOptions(options);
        int selectedIndex = _manualActorId.HasValue
            ? _dropdownTargets.FindIndex(target => target.ActorId == _manualActorId.Value) + 1
            : 0;
        _targetDropdown.SetValueWithoutNotify(Math.Max(0, selectedIndex));
        _targetDropdown.RefreshShownValue();
        _updatingDropdown = false;
        RefreshButtonState();
    }

    private void OnTargetChanged(int index)
    {
        if (_updatingDropdown)
            return;
        _manualActorId = index > 0 && index <= _dropdownTargets.Count
            ? _dropdownTargets[index - 1].ActorId
            : null;
        RefreshButtonState();
    }

    private void RefreshButtonState()
    {
        if (_catalogDirty)
        {
            foreach (ItemTile tile in _tiles)
                tile.Button.interactable = false;
            return;
        }

        bool interactable = Photon.Pun.PhotonNetwork.IsConnected && _targets.Resolve(_manualActorId) != null;
        foreach (ItemTile tile in _tiles)
            tile.Button.interactable = interactable;
        if (!interactable && _tiles.Any(tile => tile.GameObject.activeSelf))
            SetStatus(Photon.Pun.PhotonNetwork.IsConnected ? "noTarget" : "notConnected", isError: true);
    }

    private void Spawn(GameItemRecord record)
    {
        if (_targets.TrySpawn(record.Item, _manualActorId, out string errorKey))
            SetStatus(string.Empty, isError: false);
        else
            SetStatus(errorKey, isError: true);
    }

    private void OnLanguageChanged()
    {
        _catalogDirty = true;
        ApplyLocalizedChrome();
        if (isOpen)
        {
            BeginCatalogRebuild();
        }
    }

    private void OnAliasProvidersChanged()
    {
        _catalogDirty = true;
        if (isOpen)
        {
            BeginCatalogRebuild();
        }
    }

    private void ApplyLocalizedChrome()
    {
        _title.text = Localize("title");
        if (_search.placeholder is TMP_Text placeholder)
            placeholder.text = Localize("searchPlaceholder");
    }

    private string Localize(string key) => _localization.Get(GameLanguage.CurrentCode, key);

    private void SetStatus(string key, bool isError)
    {
        _status.text = string.IsNullOrEmpty(key) ? string.Empty : Localize(key);
        _status.color = isError ? RuntimeUiFactory.Error : RuntimeUiFactory.TextMuted;
    }

    private void OnDestroy()
    {
        Shutdown();
        MenuWindow.AllActiveWindows.Remove(this);
    }

    private static TMP_FontAsset ResolveFont()
    {
        if (FontFallbackSwapper.instance != null && FontFallbackSwapper.instance.mainBaseFont != null)
            return FontFallbackSwapper.instance.mainBaseFont;

        TMP_FontAsset? font = Resources.FindObjectsOfTypeAll<TMP_FontAsset>()
            .FirstOrDefault(candidate => candidate.name == "DarumaDropOne-Regular SDF") ??
            Resources.FindObjectsOfTypeAll<TMP_FontAsset>().FirstOrDefault();
        if (font == null)
            throw new InvalidOperationException("No TextMeshPro font asset is available.");
        return font;
    }

    private static void BuildView(RectTransform root, TMP_FontAsset font, out ViewReferences references)
    {
        RectTransform dim = RuntimeUiFactory.CreateRect("Dim", root, typeof(Image));
        RuntimeUiFactory.Stretch(dim);
        dim.GetComponent<Image>().color = RuntimeUiFactory.Backdrop;

        RectTransform panel = RuntimeUiFactory.CreateRect("Panel", root, typeof(Image));
        panel.anchorMin = new Vector2(0.5f, 0.5f);
        panel.anchorMax = new Vector2(0.5f, 0.5f);
        panel.pivot = new Vector2(0.5f, 0.5f);
        panel.sizeDelta = new Vector2(1180, 780);
        panel.GetComponent<Image>().color = RuntimeUiFactory.Panel;

        TextMeshProUGUI title = RuntimeUiFactory.CreateText(
            "Title", panel, font, 31, RuntimeUiFactory.TextPrimary, TextAlignmentOptions.MidlineLeft);
        title.rectTransform.anchorMin = new Vector2(0, 1);
        title.rectTransform.anchorMax = new Vector2(1, 1);
        title.rectTransform.pivot = new Vector2(0.5f, 1);
        title.rectTransform.anchoredPosition = new Vector2(0, -18);
        title.rectTransform.sizeDelta = new Vector2(-120, 48);

        Button close = RuntimeUiFactory.CreateTextButton("Close", panel, font, "X", () => { });
        RectTransform closeRect = close.GetComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(1, 1);
        closeRect.anchorMax = new Vector2(1, 1);
        closeRect.pivot = new Vector2(1, 1);
        closeRect.anchoredPosition = new Vector2(-16, -16);
        closeRect.sizeDelta = new Vector2(44, 44);

        RectTransform toolbar = RuntimeUiFactory.CreateRect("Toolbar", panel, typeof(HorizontalLayoutGroup));
        toolbar.anchorMin = new Vector2(0.5f, 1);
        toolbar.anchorMax = new Vector2(0.5f, 1);
        toolbar.pivot = new Vector2(0.5f, 1);
        toolbar.anchoredPosition = new Vector2(0, -78);
        toolbar.sizeDelta = new Vector2(1050, 52);
        HorizontalLayoutGroup horizontal = toolbar.GetComponent<HorizontalLayoutGroup>();
        horizontal.spacing = 12;
        horizontal.childControlWidth = true;
        horizontal.childControlHeight = true;
        horizontal.childForceExpandWidth = false;
        horizontal.childForceExpandHeight = true;

        TMP_InputField search = RuntimeUiFactory.CreateInputField(toolbar, font);
        TMP_Dropdown dropdown = RuntimeUiFactory.CreateDropdown(toolbar, font);

        TextMeshProUGUI status = RuntimeUiFactory.CreateText(
            "Status", panel, font, 18, RuntimeUiFactory.TextMuted, TextAlignmentOptions.MidlineLeft);
        status.rectTransform.anchorMin = new Vector2(0, 1);
        status.rectTransform.anchorMax = new Vector2(1, 1);
        status.rectTransform.pivot = new Vector2(0.5f, 1);
        status.rectTransform.anchoredPosition = new Vector2(0, -137);
        status.rectTransform.sizeDelta = new Vector2(-56, 26);

        (ScrollRect scroll, RectTransform content) = RuntimeUiFactory.CreateItemScroll(panel);
        RectTransform scrollRect = scroll.GetComponent<RectTransform>();
        scrollRect.anchorMin = Vector2.zero;
        scrollRect.anchorMax = Vector2.one;
        scrollRect.offsetMin = new Vector2(24, 24);
        scrollRect.offsetMax = new Vector2(-24, -166);

        references = new ViewReferences(title, search, dropdown, status, content, close);
    }

    private readonly struct ViewReferences
    {
        public ViewReferences(
            TextMeshProUGUI title,
            TMP_InputField search,
            TMP_Dropdown targetDropdown,
            TextMeshProUGUI status,
            RectTransform itemContent,
            Button close)
        {
            Title = title;
            Search = search;
            TargetDropdown = targetDropdown;
            Status = status;
            ItemContent = itemContent;
            Close = close;
        }

        public TextMeshProUGUI Title { get; }
        public TMP_InputField Search { get; }
        public TMP_Dropdown TargetDropdown { get; }
        public TextMeshProUGUI Status { get; }
        public RectTransform ItemContent { get; }
        public Button Close { get; }
    }
}
