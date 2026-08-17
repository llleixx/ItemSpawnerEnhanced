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
        try
        {
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

            ItemSpawnerViewReferences references = ItemSpawnerViewBuilder.Build(rootRect);
            return new ItemSpawnerView(
                root,
                references.Font,
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
        catch
        {
            root.SetActive(false);
            UnityEngine.Object.Destroy(root);
            throw;
        }
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
        ItemTile tile = ItemBrowserControlFactory.CreateItemTile(
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

}
