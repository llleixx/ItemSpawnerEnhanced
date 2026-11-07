using ItemSpawnerEnhanced.Core;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ItemSpawnerEnhanced.UI;

internal static class ItemBrowserControlFactory
{
    public static TagToggle CreateTagToggle(
        Transform parent,
        TMP_FontAsset font,
        ItemFilterTag tag,
        float iconTagWidth)
    {
        RectTransform root = RuntimeUiFactory.CreateRect(
            tag.ToString(), parent, typeof(Image), typeof(Toggle), typeof(LayoutElement));
        LayoutElement layout = root.GetComponent<LayoutElement>();
        layout.minHeight = 52;
        layout.flexibleWidth = 1;

        Image background = root.GetComponent<Image>();
        RuntimeUiFactory.ApplyRoundedCorners(background);
        Toggle toggle = root.GetComponent<Toggle>();
        toggle.targetGraphic = background;
        toggle.graphic = null;

        TextMeshProUGUI? label = null;
        RawImage? icon = null;
        if (tag is ItemFilterTag.Favorite or ItemFilterTag.Modded)
        {
            layout.minWidth = iconTagWidth;
            layout.preferredWidth = iconTagWidth;
            layout.flexibleWidth = 0;
            RectTransform iconRect = RuntimeUiFactory.CreateRect("Icon", root, typeof(RawImage));
            iconRect.anchorMin = iconRect.anchorMax = iconRect.pivot = new Vector2(0.5f, 0.5f);
            iconRect.sizeDelta = new Vector2(26, 26);
            icon = iconRect.GetComponent<RawImage>();
            icon.texture = tag == ItemFilterTag.Favorite
                ? RuntimeUiAssets.HeartTexture : RuntimeUiAssets.PuzzleTexture;
            icon.color = RuntimeUiFactory.TextPrimary;
            icon.raycastTarget = false;
        }
        else
        {
            label = RuntimeUiFactory.CreateText(
                "Label", root, font, 17, RuntimeUiFactory.TextPrimary, TextAlignmentOptions.Center);
            label.maxVisibleLines = 1;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            RuntimeUiFactory.Stretch(label.rectTransform, 5, 5, 2, 2);
        }

        var result = new TagToggle(tag, toggle, background, label, icon);
        result.SetActive(false);
        return result;
    }

    public static ItemNameTooltip CreateItemTooltip(RectTransform parent, TMP_FontAsset font)
    {
        RectTransform visual = RuntimeUiFactory.CreateRect("ItemTooltip", parent, typeof(Image));
        visual.anchorMin = new Vector2(0.5f, 0.5f);
        visual.anchorMax = new Vector2(0.5f, 0.5f);
        visual.pivot = new Vector2(0, 1);
        Image background = visual.GetComponent<Image>();
        RuntimeUiFactory.ApplyRoundedCorners(background);
        background.color = new Color(0.27f, 0.29f, 0.30f, 0.98f);
        background.raycastTarget = false;

        TextMeshProUGUI label = RuntimeUiFactory.CreateText(
            "Label", visual, font, 20, RuntimeUiFactory.TextPrimary, TextAlignmentOptions.MidlineLeft);
        label.overflowMode = TextOverflowModes.Overflow;
        RuntimeUiFactory.Stretch(label.rectTransform, 12, 12, 8, 8);

        ItemNameTooltip tooltip = parent.gameObject.AddComponent<ItemNameTooltip>();
        tooltip.Configure(parent, visual, label);
        return tooltip;
    }

    public static ItemTile CreateItemTile(
        Transform parent,
        TMP_FontAsset font,
        GameItemRecord record,
        UnityAction onClick,
        UnityAction onFavorite,
        bool isFavorite,
        ItemNameTooltip tooltip)
    {
        RectTransform root = RuntimeUiFactory.CreateRect(
            record.Item.name, parent, typeof(Image), typeof(Button));
        Image background = root.GetComponent<Image>();
        RuntimeUiFactory.ApplyRoundedCorners(background);
        background.color = RuntimeUiFactory.Surface;
        Button button = root.GetComponent<Button>();
        button.targetGraphic = background;
        button.colors = RuntimeUiFactory.CreateButtonColors();
        ItemSpawnTrigger spawnTrigger = root.gameObject.AddComponent<ItemSpawnTrigger>();
        spawnTrigger.Configure(button, onClick);

        RectTransform iconRect = RuntimeUiFactory.CreateRect("Icon", root, typeof(RawImage));
        iconRect.anchorMin = new Vector2(0.5f, 1);
        iconRect.anchorMax = new Vector2(0.5f, 1);
        iconRect.pivot = new Vector2(0.5f, 1);
        iconRect.anchoredPosition = new Vector2(0, -10);
        iconRect.sizeDelta = new Vector2(96, 96);
        RawImage icon = iconRect.GetComponent<RawImage>();
        icon.raycastTarget = false;

        TextMeshProUGUI label = RuntimeUiFactory.CreateText(
            "Name", root, font, 19, RuntimeUiFactory.TextPrimary, TextAlignmentOptions.Center);
        label.text = record.DisplayName;
        label.maxVisibleLines = 2;
        label.rectTransform.anchorMin = new Vector2(0, 0);
        label.rectTransform.anchorMax = new Vector2(1, 0);
        label.rectTransform.pivot = new Vector2(0.5f, 0);
        label.rectTransform.anchoredPosition = new Vector2(0, 6);
        label.rectTransform.sizeDelta = new Vector2(-14, 38);

        RectTransform favoriteMarker = RuntimeUiFactory.CreateRect("Favorite", root);
        favoriteMarker.anchorMin = new Vector2(1, 1);
        favoriteMarker.anchorMax = new Vector2(1, 1);
        favoriteMarker.pivot = new Vector2(1, 1);
        favoriteMarker.anchoredPosition = new Vector2(-6, -6);
        favoriteMarker.sizeDelta = new Vector2(32, 32);

        RectTransform favoriteHeart = RuntimeUiFactory.CreateRect("Heart", favoriteMarker, typeof(RawImage));
        RuntimeUiFactory.Stretch(favoriteHeart, 3, 3, 3, 3);
        RawImage favoriteHeartImage = favoriteHeart.GetComponent<RawImage>();
        favoriteHeartImage.texture = RuntimeUiAssets.HeartTexture;
        favoriteHeartImage.color = RuntimeUiFactory.Favorite;
        favoriteHeartImage.raycastTarget = false;
        favoriteMarker.gameObject.SetActive(isFavorite);

        if (record.Origin == ItemOrigin.Modded)
        {
            RectTransform moddedMarker = RuntimeUiFactory.CreateRect("Modded", root);
            moddedMarker.anchorMin = moddedMarker.anchorMax = moddedMarker.pivot = new Vector2(0, 1);
            moddedMarker.anchoredPosition = new Vector2(6, -6);
            moddedMarker.sizeDelta = new Vector2(32, 32);
            RectTransform puzzle = RuntimeUiFactory.CreateRect("Puzzle", moddedMarker, typeof(RawImage));
            RuntimeUiFactory.Stretch(puzzle, 3, 3, 3, 3);
            RawImage puzzleImage = puzzle.GetComponent<RawImage>();
            puzzleImage.texture = RuntimeUiAssets.PuzzleTexture;
            puzzleImage.color = new Color32(0x7D, 0xBB, 0xE6, 0xFF);
            puzzleImage.raycastTarget = false;
        }

        ItemFavoriteTrigger favoriteTrigger = root.gameObject.AddComponent<ItemFavoriteTrigger>();
        favoriteTrigger.Configure(onFavorite);
        root.gameObject.AddComponent<ItemNameTooltipTrigger>().Configure(tooltip, record.DisplayName);
        return new ItemTile(
            record,
            root.gameObject,
            button,
            background,
            label,
            icon,
            favoriteMarker.gameObject,
            favoriteTrigger,
            spawnTrigger);
    }
}
