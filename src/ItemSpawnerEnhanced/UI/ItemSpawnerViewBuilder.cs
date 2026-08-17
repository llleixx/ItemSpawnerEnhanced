using System;
using System.Collections.Generic;
using System.Linq;
using ItemSpawnerEnhanced.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ItemSpawnerEnhanced.UI;

internal static class ItemSpawnerViewBuilder
{
    public static ItemSpawnerViewReferences Build(RectTransform root)
    {
        TMP_FontAsset font = ResolveFont();
        RectTransform dim = RuntimeUiFactory.CreateRect("Dim", root, typeof(Image));
        RuntimeUiFactory.Stretch(dim);
        dim.GetComponent<Image>().color = RuntimeUiFactory.Backdrop;

        RectTransform panel = RuntimeUiFactory.CreateRect("Panel", root, typeof(Image));
        panel.anchorMin = new Vector2(0.5f, 0.5f);
        panel.anchorMax = new Vector2(0.5f, 0.5f);
        panel.pivot = new Vector2(0.5f, 0.5f);
        panel.sizeDelta = new Vector2(1250, 900);
        Image panelBackground = panel.GetComponent<Image>();
        RuntimeUiFactory.ApplyRoundedCorners(panelBackground);
        panelBackground.color = RuntimeUiFactory.Panel;

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
            .Select(tag => ItemBrowserControlFactory.CreateTagToggle(tagBar, font, tag))
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

        ItemNameTooltip tooltip = ItemBrowserControlFactory.CreateItemTooltip(panel, font);
        ItemNameTooltipTrigger tagClearTooltip = tagClear.gameObject.AddComponent<ItemNameTooltipTrigger>();
        tagClearTooltip.Configure(tooltip, string.Empty);
        return new ItemSpawnerViewReferences(
            font,
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
}

internal readonly struct ItemSpawnerViewReferences
{
    public ItemSpawnerViewReferences(
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
        Font = font;
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

    public TMP_FontAsset Font { get; }
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
