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
        ItemFilterTag tag)
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

        TextMeshProUGUI label = RuntimeUiFactory.CreateText(
            "Label", root, font, 17, RuntimeUiFactory.TextPrimary, TextAlignmentOptions.Center);
        label.maxVisibleLines = 2;
        RawImage? heart = null;
        if (tag == ItemFilterTag.Favorite)
        {
            RectTransform heartRect = RuntimeUiFactory.CreateRect("Heart", root, typeof(RawImage));
            heartRect.anchorMin = new Vector2(0, 0.5f);
            heartRect.anchorMax = new Vector2(0, 0.5f);
            heartRect.pivot = new Vector2(0, 0.5f);
            heartRect.anchoredPosition = new Vector2(11, 0);
            heartRect.sizeDelta = new Vector2(18, 18);
            heart = heartRect.GetComponent<RawImage>();
            heart.texture = RuntimeUiAssets.HeartTexture;
            heart.color = RuntimeUiFactory.TextPrimary;
            heart.raycastTarget = false;
            RuntimeUiFactory.Stretch(label.rectTransform, 28, 5, 2, 2);
        }
        else
        {
            RuntimeUiFactory.Stretch(label.rectTransform, 5, 5, 2, 2);
        }

        var result = new TagToggle(tag, toggle, background, label, heart);
        result.SetSelected(false);
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
        button.onClick.AddListener(onClick);

        RectTransform iconRect = RuntimeUiFactory.CreateRect("Icon", root, typeof(RawImage));
        iconRect.anchorMin = new Vector2(0.5f, 1);
        iconRect.anchorMax = new Vector2(0.5f, 1);
        iconRect.pivot = new Vector2(0.5f, 1);
        iconRect.anchoredPosition = new Vector2(0, -10);
        iconRect.sizeDelta = new Vector2(96, 96);
        RawImage icon = iconRect.GetComponent<RawImage>();
        icon.texture = record.Item.UIData?.icon;
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

        ItemFavoriteTrigger favoriteTrigger = root.gameObject.AddComponent<ItemFavoriteTrigger>();
        favoriteTrigger.Configure(onFavorite);
        root.gameObject.AddComponent<ItemNameTooltipTrigger>().Configure(tooltip, record.DisplayName);
        return new ItemTile(record, root.gameObject, button, favoriteMarker.gameObject, favoriteTrigger);
    }
}
