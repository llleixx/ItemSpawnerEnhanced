using System;
using ItemSpawnerEnhanced.Core;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ItemSpawnerEnhanced.UI;

internal static class RuntimeUiFactory
{
    private static Texture2D? _heartTexture;
    private static Texture2D? _filterClearTexture;
    private static Texture2D? _searchClearTexture;
    private static Sprite? _roundedRectSprite;

    internal static readonly Color Backdrop = new(0.035f, 0.043f, 0.047f, 0.88f);
    internal static readonly Color Panel = new(0.10f, 0.115f, 0.12f, 0.98f);
    internal static readonly Color Surface = new(0.16f, 0.175f, 0.18f, 1f);
    internal static readonly Color SurfaceHover = new(0.22f, 0.235f, 0.235f, 1f);
    internal static readonly Color Accent = new(0.30f, 0.78f, 0.58f, 1f);
    internal static readonly Color TextPrimary = new(0.95f, 0.96f, 0.94f, 1f);
    internal static readonly Color TextMuted = new(0.68f, 0.71f, 0.69f, 1f);
    internal static readonly Color Error = new(0.94f, 0.52f, 0.42f, 1f);
    internal static readonly Color Favorite = new(0.95f, 0.38f, 0.52f, 1f);

    public static RectTransform CreateRect(string name, Transform parent, params Type[] components)
    {
        Type[] componentTypes = new Type[components.Length + 1];
        componentTypes[0] = typeof(RectTransform);
        Array.Copy(components, 0, componentTypes, 1, components.Length);
        var gameObject = new GameObject(name, componentTypes);
        gameObject.transform.SetParent(parent, false);
        return (RectTransform)gameObject.transform;
    }

    public static void Stretch(RectTransform rect, float left = 0, float right = 0, float bottom = 0, float top = 0)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(left, bottom);
        rect.offsetMax = new Vector2(-right, -top);
    }

    public static void ApplyRoundedCorners(Image image)
    {
        image.sprite = RoundedRectSprite;
        image.type = Image.Type.Sliced;
    }

    public static TextMeshProUGUI CreateText(
        string name,
        Transform parent,
        TMP_FontAsset font,
        float fontSize,
        Color color,
        TextAlignmentOptions alignment)
    {
        RectTransform rect = CreateRect(name, parent, typeof(TextMeshProUGUI));
        TextMeshProUGUI text = rect.GetComponent<TextMeshProUGUI>();
        text.font = font;
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = alignment;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.overflowMode = TextOverflowModes.Ellipsis;
        text.raycastTarget = false;
        return text;
    }

    public static Button CreateTextButton(
        string name,
        Transform parent,
        TMP_FontAsset font,
        string label)
    {
        RectTransform rect = CreateRect(name, parent, typeof(Image), typeof(Button));
        Image image = rect.GetComponent<Image>();
        ApplyRoundedCorners(image);
        image.color = Surface;
        Button button = rect.GetComponent<Button>();
        button.targetGraphic = image;
        button.colors = ButtonColors();

        TextMeshProUGUI text = CreateText("Label", rect, font, 25, TextPrimary, TextAlignmentOptions.Center);
        text.text = label;
        Stretch(text.rectTransform, 4, 4, 2, 2);
        return button;
    }

    public static TMP_InputField CreateInputField(Transform parent, TMP_FontAsset font)
    {
        RectTransform root = CreateRect("Search", parent, typeof(Image), typeof(TMP_InputField), typeof(LayoutElement));
        Image background = root.GetComponent<Image>();
        ApplyRoundedCorners(background);
        background.color = Surface;
        LayoutElement layout = root.GetComponent<LayoutElement>();
        layout.minWidth = 420;
        layout.preferredWidth = 700;
        layout.flexibleWidth = 1;
        layout.minHeight = 52;

        RectTransform viewport = CreateRect("Text Area", root, typeof(RectMask2D));
        Stretch(viewport, 18, 54, 5, 5);

        TextMeshProUGUI placeholder = CreateText("Placeholder", viewport, font, 24, TextMuted, TextAlignmentOptions.MidlineLeft);
        placeholder.fontStyle = FontStyles.Italic;
        Stretch(placeholder.rectTransform);

        TextMeshProUGUI value = CreateText("Text", viewport, font, 24, TextPrimary, TextAlignmentOptions.MidlineLeft);
        value.textWrappingMode = TextWrappingModes.NoWrap;
        value.overflowMode = TextOverflowModes.Overflow;
        Stretch(value.rectTransform);

        TMP_InputField input = root.GetComponent<TMP_InputField>();
        input.textViewport = viewport;
        input.textComponent = value;
        input.placeholder = placeholder;
        input.lineType = TMP_InputField.LineType.SingleLine;
        input.caretColor = Accent;
        input.selectionColor = new Color(Accent.r, Accent.g, Accent.b, 0.35f);
        input.customCaretColor = true;
        return input;
    }

    public static Button CreateSearchClearButton(Transform parent)
    {
        RectTransform rect = CreateRect("ClearSearch", parent, typeof(Image), typeof(Button));
        Image background = rect.GetComponent<Image>();
        ApplyRoundedCorners(background);
        background.color = Color.white;
        Button button = rect.GetComponent<Button>();
        button.targetGraphic = background;
        button.colors = ButtonColors();

        RectTransform iconRect = CreateRect("Icon", rect, typeof(RawImage));
        Stretch(iconRect, 9, 9, 9, 9);
        RawImage icon = iconRect.GetComponent<RawImage>();
        icon.texture = SearchClearTexture;
        icon.color = TextPrimary;
        icon.raycastTarget = false;

        rect.anchorMin = new Vector2(1, 0.5f);
        rect.anchorMax = new Vector2(1, 0.5f);
        rect.pivot = new Vector2(1, 0.5f);
        rect.anchoredPosition = new Vector2(-6, 0);
        rect.sizeDelta = new Vector2(40, 40);
        button.gameObject.SetActive(false);
        return button;
    }

    public static (Button Button, RawImage Icon) CreateFilterClearButton(Transform parent)
    {
        RectTransform root = CreateRect("ClearTags", parent, typeof(Image), typeof(Button));
        Image background = root.GetComponent<Image>();
        ApplyRoundedCorners(background);
        background.color = Surface;
        Button button = root.GetComponent<Button>();
        button.targetGraphic = background;
        button.colors = ButtonColors();

        RectTransform iconRect = CreateRect("Icon", root, typeof(RawImage));
        Stretch(iconRect, 8, 8, 10, 10);
        RawImage icon = iconRect.GetComponent<RawImage>();
        icon.texture = FilterClearTexture;
        icon.color = TextPrimary;
        icon.raycastTarget = false;
        return (button, icon);
    }

    public static TMP_Dropdown CreateDropdown(Transform parent, TMP_FontAsset font)
    {
        RectTransform root = CreateRect("Target", parent, typeof(Image), typeof(TMP_Dropdown), typeof(LayoutElement));
        Image rootBackground = root.GetComponent<Image>();
        ApplyRoundedCorners(rootBackground);
        rootBackground.color = Surface;
        LayoutElement layout = root.GetComponent<LayoutElement>();
        layout.minWidth = 240;
        layout.preferredWidth = 280;
        layout.flexibleWidth = 0;
        layout.minHeight = 52;

        TextMeshProUGUI caption = CreateText("Label", root, font, 22, TextPrimary, TextAlignmentOptions.MidlineLeft);
        Stretch(caption.rectTransform, 16, 42, 3, 3);
        TextMeshProUGUI arrow = CreateText("Arrow", root, font, 20, Accent, TextAlignmentOptions.Center);
        arrow.text = "v";
        arrow.rectTransform.anchorMin = new Vector2(1, 0.5f);
        arrow.rectTransform.anchorMax = new Vector2(1, 0.5f);
        arrow.rectTransform.pivot = new Vector2(1, 0.5f);
        arrow.rectTransform.anchoredPosition = new Vector2(-10, 0);
        arrow.rectTransform.sizeDelta = new Vector2(30, 40);

        RectTransform template = CreateRect("Template", root, typeof(Image), typeof(ScrollRect));
        template.anchorMin = new Vector2(0, 0);
        template.anchorMax = new Vector2(1, 0);
        template.pivot = new Vector2(0.5f, 1);
        template.anchoredPosition = new Vector2(0, -4);
        template.sizeDelta = new Vector2(0, 250);
        Image templateBackground = template.GetComponent<Image>();
        ApplyRoundedCorners(templateBackground);
        templateBackground.color = new Color(0.08f, 0.09f, 0.095f, 1f);

        RectTransform viewport = CreateRect("Viewport", template, typeof(Image), typeof(Mask));
        Stretch(viewport, 4, 4, 4, 4);
        viewport.GetComponent<Image>().color = Color.white;
        viewport.GetComponent<Mask>().showMaskGraphic = false;

        RectTransform content = CreateRect("Content", viewport, typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        content.anchorMin = new Vector2(0, 1);
        content.anchorMax = new Vector2(1, 1);
        content.pivot = new Vector2(0.5f, 1);
        content.anchoredPosition = Vector2.zero;
        content.sizeDelta = Vector2.zero;
        VerticalLayoutGroup vertical = content.GetComponent<VerticalLayoutGroup>();
        vertical.childControlHeight = true;
        vertical.childControlWidth = true;
        vertical.childForceExpandHeight = false;
        vertical.childForceExpandWidth = true;
        vertical.spacing = 2;
        content.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        RectTransform item = CreateRect("Item", content, typeof(Toggle), typeof(LayoutElement));
        item.GetComponent<LayoutElement>().preferredHeight = 40;
        RectTransform itemBackground = CreateRect("Item Background", item, typeof(Image));
        Stretch(itemBackground);
        Image itemBackgroundImage = itemBackground.GetComponent<Image>();
        ApplyRoundedCorners(itemBackgroundImage);
        itemBackgroundImage.color = Surface;
        TextMeshProUGUI itemLabel = CreateText("Item Label", item, font, 20, TextPrimary, TextAlignmentOptions.MidlineLeft);
        Stretch(itemLabel.rectTransform, 14, 10, 2, 2);
        Toggle toggle = item.GetComponent<Toggle>();
        toggle.targetGraphic = itemBackground.GetComponent<Image>();
        toggle.graphic = null;

        ScrollRect scroll = template.GetComponent<ScrollRect>();
        scroll.content = content;
        scroll.viewport = viewport;
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 25;

        TMP_Dropdown dropdown = root.GetComponent<TMP_Dropdown>();
        dropdown.targetGraphic = root.GetComponent<Image>();
        dropdown.template = template;
        dropdown.captionText = caption;
        dropdown.itemText = itemLabel;
        dropdown.options.Clear();
        template.gameObject.SetActive(false);
        return dropdown;
    }

    public static (ScrollRect Scroll, RectTransform Content) CreateItemScroll(Transform parent)
    {
        RectTransform root = CreateRect("Items", parent, typeof(ScrollRect));
        RectTransform viewport = CreateRect("Viewport", root, typeof(Image), typeof(Mask));
        Stretch(viewport);
        viewport.GetComponent<Image>().color = new Color(1, 1, 1, 0.01f);
        viewport.GetComponent<Mask>().showMaskGraphic = false;

        RectTransform content = CreateRect("Content", viewport, typeof(GridLayoutGroup), typeof(ContentSizeFitter));
        content.anchorMin = new Vector2(0, 1);
        content.anchorMax = new Vector2(1, 1);
        content.pivot = new Vector2(0.5f, 1);
        content.anchoredPosition = Vector2.zero;
        content.sizeDelta = Vector2.zero;
        GridLayoutGroup grid = content.GetComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(176, 150);
        grid.spacing = new Vector2(12, 12);
        grid.padding = new RectOffset(4, 4, 4, 4);
        grid.constraint = GridLayoutGroup.Constraint.Flexible;
        grid.startAxis = GridLayoutGroup.Axis.Horizontal;
        grid.childAlignment = TextAnchor.UpperCenter;
        content.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        ScrollRect scroll = root.GetComponent<ScrollRect>();
        scroll.viewport = viewport;
        scroll.content = content;
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 40;
        return (scroll, content);
    }

    public static TagToggle CreateTagToggle(
        Transform parent,
        TMP_FontAsset font,
        ItemFilterTag tag)
    {
        RectTransform root = CreateRect(tag.ToString(), parent, typeof(Image), typeof(Toggle), typeof(LayoutElement));
        LayoutElement layout = root.GetComponent<LayoutElement>();
        layout.minHeight = 52;
        layout.flexibleWidth = 1;

        Image background = root.GetComponent<Image>();
        ApplyRoundedCorners(background);
        Toggle toggle = root.GetComponent<Toggle>();
        toggle.targetGraphic = background;
        toggle.graphic = null;

        TextMeshProUGUI label = CreateText(
            "Label", root, font, 17, TextPrimary, TextAlignmentOptions.Center);
        label.maxVisibleLines = 2;
        RawImage? heart = null;
        if (tag == ItemFilterTag.Favorite)
        {
            RectTransform heartRect = CreateRect("Heart", root, typeof(RawImage));
            heartRect.anchorMin = new Vector2(0, 0.5f);
            heartRect.anchorMax = new Vector2(0, 0.5f);
            heartRect.pivot = new Vector2(0, 0.5f);
            heartRect.anchoredPosition = new Vector2(11, 0);
            heartRect.sizeDelta = new Vector2(18, 18);
            heart = heartRect.GetComponent<RawImage>();
            heart.texture = HeartTexture;
            heart.color = TextPrimary;
            heart.raycastTarget = false;
            Stretch(label.rectTransform, 28, 5, 2, 2);
        }
        else
        {
            Stretch(label.rectTransform, 5, 5, 2, 2);
        }

        var result = new TagToggle(tag, toggle, background, label, heart);
        result.SetSelected(false);
        return result;
    }

    public static ItemNameTooltip CreateItemTooltip(RectTransform parent, TMP_FontAsset font)
    {
        RectTransform visual = CreateRect("ItemTooltip", parent, typeof(Image));
        visual.anchorMin = new Vector2(0.5f, 0.5f);
        visual.anchorMax = new Vector2(0.5f, 0.5f);
        visual.pivot = new Vector2(0, 1);
        Image background = visual.GetComponent<Image>();
        ApplyRoundedCorners(background);
        background.color = new Color(0.27f, 0.29f, 0.30f, 0.98f);
        background.raycastTarget = false;

        TextMeshProUGUI label = CreateText(
            "Label", visual, font, 20, TextPrimary, TextAlignmentOptions.MidlineLeft);
        label.overflowMode = TextOverflowModes.Overflow;
        Stretch(label.rectTransform, 12, 12, 8, 8);

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
        RectTransform root = CreateRect(record.Item.name, parent, typeof(Image), typeof(Button));
        Image background = root.GetComponent<Image>();
        ApplyRoundedCorners(background);
        background.color = Surface;
        Button button = root.GetComponent<Button>();
        button.targetGraphic = background;
        button.colors = ButtonColors();
        button.onClick.AddListener(onClick);

        RectTransform iconRect = CreateRect("Icon", root, typeof(RawImage));
        iconRect.anchorMin = new Vector2(0.5f, 1);
        iconRect.anchorMax = new Vector2(0.5f, 1);
        iconRect.pivot = new Vector2(0.5f, 1);
        iconRect.anchoredPosition = new Vector2(0, -10);
        iconRect.sizeDelta = new Vector2(96, 96);
        RawImage icon = iconRect.GetComponent<RawImage>();
        icon.texture = record.Item.UIData?.icon;
        icon.raycastTarget = false;

        TextMeshProUGUI label = CreateText("Name", root, font, 19, TextPrimary, TextAlignmentOptions.Center);
        label.text = record.DisplayName;
        label.maxVisibleLines = 2;
        label.rectTransform.anchorMin = new Vector2(0, 0);
        label.rectTransform.anchorMax = new Vector2(1, 0);
        label.rectTransform.pivot = new Vector2(0.5f, 0);
        label.rectTransform.anchoredPosition = new Vector2(0, 6);
        label.rectTransform.sizeDelta = new Vector2(-14, 38);

        RectTransform favoriteMarker = CreateRect("Favorite", root);
        favoriteMarker.anchorMin = new Vector2(1, 1);
        favoriteMarker.anchorMax = new Vector2(1, 1);
        favoriteMarker.pivot = new Vector2(1, 1);
        favoriteMarker.anchoredPosition = new Vector2(-6, -6);
        favoriteMarker.sizeDelta = new Vector2(32, 32);

        RectTransform favoriteHeart = CreateRect("Heart", favoriteMarker, typeof(RawImage));
        Stretch(favoriteHeart, 3, 3, 3, 3);
        RawImage favoriteHeartImage = favoriteHeart.GetComponent<RawImage>();
        favoriteHeartImage.texture = HeartTexture;
        favoriteHeartImage.color = Favorite;
        favoriteHeartImage.raycastTarget = false;
        favoriteMarker.gameObject.SetActive(isFavorite);

        ItemFavoriteTrigger favoriteTrigger = root.gameObject.AddComponent<ItemFavoriteTrigger>();
        favoriteTrigger.Configure(onFavorite);
        root.gameObject.AddComponent<ItemNameTooltipTrigger>().Configure(tooltip, record.DisplayName);
        return new ItemTile(record, root.gameObject, button, favoriteMarker.gameObject, favoriteTrigger);
    }

    private static ColorBlock ButtonColors()
    {
        ColorBlock colors = ColorBlock.defaultColorBlock;
        colors.normalColor = Surface;
        colors.highlightedColor = SurfaceHover;
        colors.pressedColor = Accent;
        colors.selectedColor = SurfaceHover;
        colors.disabledColor = new Color(Surface.r, Surface.g, Surface.b, 0.45f);
        colors.colorMultiplier = 1;
        colors.fadeDuration = 0.08f;
        return colors;
    }

    private static Texture2D HeartTexture => _heartTexture ??= CreateHeartTexture();

    private static Texture2D FilterClearTexture => _filterClearTexture ??= CreateFilterClearTexture();

    private static Texture2D SearchClearTexture => _searchClearTexture ??= CreateSearchClearTexture();

    private static Sprite RoundedRectSprite => _roundedRectSprite ??= CreateRoundedRectSprite();

    private static Texture2D CreateHeartTexture()
    {
        const int size = 32;
        const int samplesPerAxis = 4;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, mipChain: false)
        {
            name = "ItemSpawnerEnhanced Heart",
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp,
            hideFlags = HideFlags.HideAndDontSave
        };
        var pixels = new Color32[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                int insideSamples = 0;
                for (int sampleY = 0; sampleY < samplesPerAxis; sampleY++)
                {
                    for (int sampleX = 0; sampleX < samplesPerAxis; sampleX++)
                    {
                        float pointX = x + (sampleX + 0.5f) / samplesPerAxis;
                        float pointY = y + (sampleY + 0.5f) / samplesPerAxis;
                        float normalizedX = (pointX - 16f) / 11f;
                        float normalizedY = (pointY - 14.5f) / 11f;
                        float sum = normalizedX * normalizedX + normalizedY * normalizedY - 1f;
                        if (sum * sum * sum - normalizedX * normalizedX * normalizedY * normalizedY * normalizedY <= 0f)
                            insideSamples++;
                    }
                }

                byte alpha = (byte)(255 * insideSamples / (samplesPerAxis * samplesPerAxis));
                pixels[y * size + x] = new Color32(255, 255, 255, alpha);
            }
        }

        texture.SetPixels32(pixels);
        texture.Apply(updateMipmaps: false, makeNoLongerReadable: true);
        return texture;
    }

    private static Sprite CreateRoundedRectSprite()
    {
        const int size = 32;
        const float radius = 8f;
        const int samplesPerAxis = 4;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, mipChain: false)
        {
            name = "ItemSpawnerEnhanced Rounded Rectangle",
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp,
            hideFlags = HideFlags.HideAndDontSave
        };
        var pixels = new Color32[size * size];
        Vector2 center = new(size * 0.5f, size * 0.5f);
        float straightHalfExtent = size * 0.5f - radius;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                int insideSamples = 0;
                for (int sampleY = 0; sampleY < samplesPerAxis; sampleY++)
                {
                    for (int sampleX = 0; sampleX < samplesPerAxis; sampleX++)
                    {
                        Vector2 point = new(
                            x + (sampleX + 0.5f) / samplesPerAxis,
                            y + (sampleY + 0.5f) / samplesPerAxis);
                        Vector2 distance = new(
                            Mathf.Abs(point.x - center.x) - straightHalfExtent,
                            Mathf.Abs(point.y - center.y) - straightHalfExtent);
                        Vector2 outside = new(Mathf.Max(distance.x, 0), Mathf.Max(distance.y, 0));
                        float signedDistance = outside.magnitude + Mathf.Min(Mathf.Max(distance.x, distance.y), 0) - radius;
                        if (signedDistance <= 0)
                            insideSamples++;
                    }
                }

                byte alpha = (byte)(255 * insideSamples / (samplesPerAxis * samplesPerAxis));
                pixels[y * size + x] = new Color32(255, 255, 255, alpha);
            }
        }

        texture.SetPixels32(pixels);
        texture.Apply(updateMipmaps: false, makeNoLongerReadable: true);
        Sprite sprite = Sprite.Create(
            texture,
            new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f),
            pixelsPerUnit: 100f,
            extrude: 0,
            SpriteMeshType.FullRect,
            new Vector4(9, 9, 9, 9));
        sprite.name = "ItemSpawnerEnhanced Rounded Rectangle";
        sprite.hideFlags = HideFlags.HideAndDontSave;
        return sprite;
    }

    private static Texture2D CreateFilterClearTexture() => CreateLineTexture(
        "ItemSpawnerEnhanced Clear Filters",
        new Vector2[]
        {
            new(4, 26), new(27, 26),
            new(4, 26), new(13, 16),
            new(27, 26), new(20, 18),
            new(13, 16), new(13, 5),
            new(13, 5), new(18, 8),
            new(18, 8), new(18, 14),
            new(20, 15), new(28, 7),
            new(28, 15), new(20, 7)
        });

    private static Texture2D CreateSearchClearTexture() => CreateLineTexture(
        "ItemSpawnerEnhanced Clear Search",
        new Vector2[]
        {
            new(7, 7), new(25, 25),
            new(25, 7), new(7, 25)
        });

    private static Texture2D CreateLineTexture(string name, Vector2[] segments)
    {
        const int size = 32;
        const int samplesPerAxis = 4;
        const float halfStroke = 1.35f;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, mipChain: false)
        {
            name = name,
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp,
            hideFlags = HideFlags.HideAndDontSave
        };
        var pixels = new Color32[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                int insideSamples = 0;
                for (int sampleY = 0; sampleY < samplesPerAxis; sampleY++)
                {
                    for (int sampleX = 0; sampleX < samplesPerAxis; sampleX++)
                    {
                        Vector2 point = new(
                            x + (sampleX + 0.5f) / samplesPerAxis,
                            y + (sampleY + 0.5f) / samplesPerAxis);
                        for (int segment = 0; segment < segments.Length; segment += 2)
                        {
                            if (DistanceToSegment(point, segments[segment], segments[segment + 1]) <= halfStroke)
                            {
                                insideSamples++;
                                break;
                            }
                        }
                    }
                }

                byte alpha = (byte)(255 * insideSamples / (samplesPerAxis * samplesPerAxis));
                pixels[y * size + x] = new Color32(255, 255, 255, alpha);
            }
        }

        texture.SetPixels32(pixels);
        texture.Apply(updateMipmaps: false, makeNoLongerReadable: true);
        return texture;
    }

    private static float DistanceToSegment(Vector2 point, Vector2 start, Vector2 end)
    {
        Vector2 segment = end - start;
        float lengthSquared = segment.sqrMagnitude;
        if (lengthSquared <= Mathf.Epsilon)
            return Vector2.Distance(point, start);
        float position = Mathf.Clamp01(Vector2.Dot(point - start, segment) / lengthSquared);
        return Vector2.Distance(point, start + segment * position);
    }
}

internal sealed class ItemTile
{
    public ItemTile(
        GameItemRecord record,
        GameObject gameObject,
        Button button,
        GameObject favoriteMarker,
        ItemFavoriteTrigger favoriteTrigger)
    {
        Record = record;
        GameObject = gameObject;
        Button = button;
        FavoriteMarker = favoriteMarker;
        FavoriteTrigger = favoriteTrigger;
    }

    public GameItemRecord Record { get; }
    public GameObject GameObject { get; }
    public Button Button { get; }
    public GameObject FavoriteMarker { get; }
    public ItemFavoriteTrigger FavoriteTrigger { get; }

    public void SetFavorite(bool favorite) => FavoriteMarker.SetActive(favorite);
}

internal sealed class TagToggle
{
    private readonly Image _background;
    private readonly RawImage? _heart;

    public TagToggle(
        ItemFilterTag tag,
        Toggle toggle,
        Image background,
        TMP_Text label,
        RawImage? heart)
    {
        Tag = tag;
        Toggle = toggle;
        _background = background;
        Label = label;
        _heart = heart;
    }

    public ItemFilterTag Tag { get; }
    public Toggle Toggle { get; }
    public TMP_Text Label { get; }

    public void SetSelected(bool selected)
    {
        Color normal = selected ? RuntimeUiFactory.Accent : RuntimeUiFactory.Surface;
        ColorBlock colors = ColorBlock.defaultColorBlock;
        colors.normalColor = normal;
        colors.highlightedColor = selected
            ? Color.Lerp(RuntimeUiFactory.Accent, Color.white, 0.12f)
            : RuntimeUiFactory.SurfaceHover;
        colors.pressedColor = RuntimeUiFactory.Accent;
        colors.selectedColor = colors.highlightedColor;
        colors.disabledColor = new Color(normal.r, normal.g, normal.b, 0.45f);
        colors.colorMultiplier = 1;
        colors.fadeDuration = 0.08f;
        Toggle.colors = colors;
        _background.color = normal;
        Label.color = selected ? RuntimeUiFactory.Panel : RuntimeUiFactory.TextPrimary;
        if (_heart != null)
            _heart.color = selected ? RuntimeUiFactory.Panel : RuntimeUiFactory.TextPrimary;
    }
}

internal sealed class ItemFavoriteTrigger : MonoBehaviour, IPointerClickHandler
{
    private UnityAction? _onFavorite;

    public bool InteractionEnabled { get; set; }

    public void Configure(UnityAction onFavorite) => _onFavorite = onFavorite;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (InteractionEnabled && eventData.button == PointerEventData.InputButton.Right)
            _onFavorite?.Invoke();
    }
}
