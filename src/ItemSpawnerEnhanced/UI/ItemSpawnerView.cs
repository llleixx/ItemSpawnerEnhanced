using System;
using System.Collections.Generic;
using System.Linq;
using ItemSpawnerEnhanced.Core;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ItemSpawnerEnhanced.UI;

internal sealed class ItemSpawnerView
{
    private readonly TMP_FontAsset _font;
    private readonly TMP_InputField _search;
    private readonly TMP_Dropdown _targetDropdown;
    private readonly TextMeshProUGUI _title;
    private readonly TextMeshProUGUI _status;
    private readonly RectTransform _itemContent;
    private readonly Button _close;
    private readonly Button _searchClear;
    private readonly Button _tagClear;
    private readonly RawImage _tagClearIcon;
    private readonly ItemNameTooltipTrigger _tagClearTooltip;
    private readonly ItemNameTooltip _tooltip;
    private readonly IReadOnlyList<TagToggle> _tagToggles;
    private readonly List<ItemTile> _tiles = new();
    private bool? _spawnEnabled;

    private ItemSpawnerView(
        GameObject root,
        TMP_FontAsset font,
        TextMeshProUGUI title,
        TMP_InputField search,
        TMP_Dropdown targetDropdown,
        TextMeshProUGUI status,
        RectTransform itemContent,
        Button close,
        Button searchClear,
        Button tagClear,
        RawImage tagClearIcon,
        ItemNameTooltipTrigger tagClearTooltip,
        ItemNameTooltip tooltip,
        IReadOnlyList<TagToggle> tagToggles)
    {
        Root = root;
        _font = font;
        _title = title;
        _search = search;
        _targetDropdown = targetDropdown;
        _status = status;
        _itemContent = itemContent;
        _close = close;
        _searchClear = searchClear;
        _tagClear = tagClear;
        _tagClearIcon = tagClearIcon;
        _tagClearTooltip = tagClearTooltip;
        _tooltip = tooltip;
        _tagToggles = tagToggles;
    }

    public GameObject Root { get; }
    public Selectable SearchSelectable => _search;
    public string SearchText => _search.text;
    public int VisibleItemCount => _tiles.Count(tile => tile.GameObject.activeSelf);

    public static ItemSpawnerView Create()
    {
        var root = new GameObject(
            "ItemSpawnerEnhancedUI",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster));
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
        BuildLayout(rootRect, font, out ViewReferences references);
        return new ItemSpawnerView(
            root,
            font,
            references.Title,
            references.Search,
            references.TargetDropdown,
            references.Status,
            references.ItemContent,
            references.Close,
            references.SearchClear,
            references.TagClear,
            references.TagClearIcon,
            references.TagClearTooltip,
            references.Tooltip,
            references.TagToggles);
    }

    public void Bind(
        UnityAction close,
        UnityAction<string> searchChanged,
        UnityAction<int> targetChanged,
        Action<ItemFilterTag, bool> tagChanged,
        UnityAction clearTags)
    {
        _close.onClick.AddListener(close);
        _search.onValueChanged.AddListener(query =>
        {
            UpdateSearchClearButton();
            searchChanged(query);
        });
        _searchClear.onClick.AddListener(() =>
        {
            _search.SetTextWithoutNotify(string.Empty);
            UpdateSearchClearButton();
            searchChanged(string.Empty);
            _search.ActivateInputField();
        });
        _tagClear.onClick.AddListener(() =>
        {
            _tooltip.Hide();
            clearTags();
        });
        _targetDropdown.onValueChanged.AddListener(targetChanged);
        foreach (TagToggle tagToggle in _tagToggles)
        {
            TagToggle captured = tagToggle;
            captured.Toggle.onValueChanged.AddListener(selected =>
            {
                captured.SetSelected(selected);
                tagChanged(captured.Tag, selected);
                UpdateTagClearButton();
            });
        }
    }

    public void SetLocalizedChrome(
        string title,
        string searchPlaceholder,
        string clearTagsTooltip,
        Func<ItemFilterTag, string> tagLabel)
    {
        _title.text = title;
        if (_search.placeholder is TMP_Text placeholder)
            placeholder.text = searchPlaceholder;
        _tagClearTooltip.SetText(clearTagsTooltip);
        foreach (TagToggle tagToggle in _tagToggles)
        {
            string label = tagLabel(tagToggle.Tag);
            tagToggle.Label.text = label;
        }
    }

    public void ActivateSearch() => _search.ActivateInputField();

    public void DeactivateSearch() => _search.DeactivateInputField();

    public void ClearItems()
    {
        _tooltip.Hide();
        foreach (ItemTile tile in _tiles)
        {
            tile.GameObject.SetActive(false);
            UnityEngine.Object.Destroy(tile.GameObject);
        }
        _tiles.Clear();
        _spawnEnabled = null;
    }

    public void AddItem(
        GameItemRecord record,
        UnityAction spawn,
        UnityAction toggleFavorite,
        bool isFavorite)
    {
        ItemTile tile = RuntimeUiFactory.CreateItemTile(
            _itemContent,
            _font,
            record,
            spawn,
            toggleFavorite,
            isFavorite,
            _tooltip);
        tile.Button.interactable = false;
        _tiles.Add(tile);
    }

    public void SetFavorite(GameItemRecord record, bool isFavorite)
    {
        ItemTile? tile = _tiles.FirstOrDefault(candidate => candidate.Record == record);
        tile?.SetFavorite(isFavorite);
    }

    public void SetFavoriteEnabled(bool enabled)
    {
        foreach (ItemTile tile in _tiles)
            tile.FavoriteTrigger.InteractionEnabled = enabled;
    }

    public void SetSelectedTags(ItemFilterTag selectedTags)
    {
        foreach (TagToggle tagToggle in _tagToggles)
        {
            bool selected = (selectedTags & tagToggle.Tag) != 0;
            tagToggle.Toggle.SetIsOnWithoutNotify(selected);
            tagToggle.SetSelected(selected);
        }
        UpdateTagClearButton();
    }

    private void UpdateSearchClearButton() =>
        _searchClear.gameObject.SetActive(!string.IsNullOrEmpty(_search.text));

    private void UpdateTagClearButton()
    {
        bool enabled = _tagToggles.Any(tagToggle => tagToggle.Toggle.isOn);
        _tagClear.interactable = enabled;
        _tagClearIcon.color = enabled
            ? RuntimeUiFactory.TextPrimary
            : new Color(RuntimeUiFactory.TextMuted.r, RuntimeUiFactory.TextMuted.g, RuntimeUiFactory.TextMuted.b, 0.35f);
    }

    public void ShowSearchResults(IReadOnlyList<GameItemRecord> results)
    {
        _tooltip.Hide();
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
    }

    public void SetTargets(IReadOnlyList<string> labels, int selectedIndex)
    {
        _targetDropdown.ClearOptions();
        _targetDropdown.AddOptions(labels.Select(label => new TMP_Dropdown.OptionData(label)).ToList());
        _targetDropdown.SetValueWithoutNotify(Math.Max(0, selectedIndex));
        _targetDropdown.RefreshShownValue();
    }

    public void SetSpawnEnabled(bool enabled, bool force = false)
    {
        if (!force && _spawnEnabled == enabled)
            return;

        _spawnEnabled = enabled;
        foreach (ItemTile tile in _tiles)
            tile.Button.interactable = enabled;
    }

    public void SetStatus(string text, bool isError)
    {
        _status.text = text;
        _status.color = isError ? RuntimeUiFactory.Error : RuntimeUiFactory.TextMuted;
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

    private static void BuildLayout(RectTransform root, TMP_FontAsset font, out ViewReferences references)
    {
        RectTransform dim = RuntimeUiFactory.CreateRect("Dim", root, typeof(Image));
        RuntimeUiFactory.Stretch(dim);
        dim.GetComponent<Image>().color = RuntimeUiFactory.Backdrop;

        RectTransform panel = RuntimeUiFactory.CreateRect("Panel", root, typeof(Image));
        panel.anchorMin = new Vector2(0.5f, 0.5f);
        panel.anchorMax = new Vector2(0.5f, 0.5f);
        panel.pivot = new Vector2(0.5f, 0.5f);
        panel.sizeDelta = new Vector2(1250, 900);
        panel.GetComponent<Image>().color = RuntimeUiFactory.Panel;

        TextMeshProUGUI title = RuntimeUiFactory.CreateText(
            "Title", panel, font, 31, RuntimeUiFactory.TextPrimary, TextAlignmentOptions.MidlineLeft);
        title.rectTransform.anchorMin = new Vector2(0, 1);
        title.rectTransform.anchorMax = new Vector2(1, 1);
        title.rectTransform.pivot = new Vector2(0.5f, 1);
        title.rectTransform.anchoredPosition = new Vector2(0, -18);
        title.rectTransform.sizeDelta = new Vector2(-120, 48);

        Button close = RuntimeUiFactory.CreateTextButton("Close", panel, font, "X");
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
        toolbar.sizeDelta = new Vector2(1120, 52);
        HorizontalLayoutGroup horizontal = toolbar.GetComponent<HorizontalLayoutGroup>();
        horizontal.spacing = 12;
        horizontal.childControlWidth = true;
        horizontal.childControlHeight = true;
        horizontal.childForceExpandWidth = false;
        horizontal.childForceExpandHeight = true;

        TMP_InputField search = RuntimeUiFactory.CreateInputField(toolbar, font);
        Button searchClear = RuntimeUiFactory.CreateSearchClearButton(search.transform);
        TMP_Dropdown dropdown = RuntimeUiFactory.CreateDropdown(toolbar, font);

        RectTransform tagBar = RuntimeUiFactory.CreateRect("Tags", panel, typeof(HorizontalLayoutGroup));
        tagBar.anchorMin = new Vector2(0.5f, 1);
        tagBar.anchorMax = new Vector2(0.5f, 1);
        tagBar.pivot = new Vector2(0.5f, 1);
        tagBar.anchoredPosition = new Vector2(0, -148);
        tagBar.sizeDelta = new Vector2(1120, 52);
        HorizontalLayoutGroup tagLayout = tagBar.GetComponent<HorizontalLayoutGroup>();
        tagLayout.spacing = 8;
        tagLayout.childControlWidth = true;
        tagLayout.childControlHeight = true;
        tagLayout.childForceExpandWidth = true;
        tagLayout.childForceExpandHeight = true;

        ItemFilterTag[] filterTags =
        {
            ItemFilterTag.Favorite,
            ItemFilterTag.Food,
            ItemFilterTag.Consumable,
            ItemFilterTag.Equipment,
            ItemFilterTag.Deployable,
            ItemFilterTag.Mystical,
            ItemFilterTag.Other
        };
        TagToggle[] tagToggles = filterTags
            .Select(tag => RuntimeUiFactory.CreateTagToggle(tagBar, font, tag))
            .ToArray();

        (Button tagClear, RawImage tagClearIcon) = RuntimeUiFactory.CreateFilterClearButton(panel);
        RectTransform tagClearRect = tagClear.GetComponent<RectTransform>();
        tagClearRect.anchorMin = new Vector2(0.5f, 1);
        tagClearRect.anchorMax = new Vector2(0.5f, 1);
        tagClearRect.pivot = new Vector2(0, 1);
        tagClearRect.anchoredPosition = new Vector2(568, -148);
        tagClearRect.sizeDelta = new Vector2(44, 52);

        TextMeshProUGUI status = RuntimeUiFactory.CreateText(
            "Status", panel, font, 18, RuntimeUiFactory.TextMuted, TextAlignmentOptions.MidlineLeft);
        status.rectTransform.anchorMin = new Vector2(0, 1);
        status.rectTransform.anchorMax = new Vector2(1, 1);
        status.rectTransform.pivot = new Vector2(0.5f, 1);
        status.rectTransform.anchoredPosition = new Vector2(0, -201);
        status.rectTransform.sizeDelta = new Vector2(-56, 18);

        (ScrollRect scroll, RectTransform content) = RuntimeUiFactory.CreateItemScroll(panel);
        RectTransform scrollRect = scroll.GetComponent<RectTransform>();
        scrollRect.anchorMin = Vector2.zero;
        scrollRect.anchorMax = Vector2.one;
        scrollRect.offsetMin = new Vector2(24, 24);
        scrollRect.offsetMax = new Vector2(-24, -221);

        ItemNameTooltip tooltip = RuntimeUiFactory.CreateItemTooltip(panel, font);
        ItemNameTooltipTrigger tagClearTooltip = tagClear.gameObject.AddComponent<ItemNameTooltipTrigger>();
        tagClearTooltip.Configure(tooltip, string.Empty);
        references = new ViewReferences(
            title,
            search,
            dropdown,
            status,
            content,
            close,
            searchClear,
            tagClear,
            tagClearIcon,
            tagClearTooltip,
            tooltip,
            tagToggles);
    }

    private readonly struct ViewReferences
    {
        public ViewReferences(
            TextMeshProUGUI title,
            TMP_InputField search,
            TMP_Dropdown targetDropdown,
            TextMeshProUGUI status,
            RectTransform itemContent,
            Button close,
            Button searchClear,
            Button tagClear,
            RawImage tagClearIcon,
            ItemNameTooltipTrigger tagClearTooltip,
            ItemNameTooltip tooltip,
            IReadOnlyList<TagToggle> tagToggles)
        {
            Title = title;
            Search = search;
            TargetDropdown = targetDropdown;
            Status = status;
            ItemContent = itemContent;
            Close = close;
            SearchClear = searchClear;
            TagClear = tagClear;
            TagClearIcon = tagClearIcon;
            TagClearTooltip = tagClearTooltip;
            Tooltip = tooltip;
            TagToggles = tagToggles;
        }

        public TextMeshProUGUI Title { get; }
        public TMP_InputField Search { get; }
        public TMP_Dropdown TargetDropdown { get; }
        public TextMeshProUGUI Status { get; }
        public RectTransform ItemContent { get; }
        public Button Close { get; }
        public Button SearchClear { get; }
        public Button TagClear { get; }
        public RawImage TagClearIcon { get; }
        public ItemNameTooltipTrigger TagClearTooltip { get; }
        public ItemNameTooltip Tooltip { get; }
        public IReadOnlyList<TagToggle> TagToggles { get; }
    }
}
