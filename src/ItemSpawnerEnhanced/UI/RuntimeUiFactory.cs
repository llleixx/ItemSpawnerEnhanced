using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ItemSpawnerEnhanced.UI;

internal static class RuntimeUiFactory
{
    internal static readonly Color Backdrop = new(0.035f, 0.043f, 0.047f, 0.88f);
    internal static readonly Color Panel = new(0.10f, 0.115f, 0.12f, 0.98f);
    internal static readonly Color Surface = new(0.16f, 0.175f, 0.18f, 1f);
    internal static readonly Color SurfaceHover = new(0.22f, 0.235f, 0.235f, 1f);
    internal static readonly Color Accent = new(0.30f, 0.78f, 0.58f, 1f);
    internal static readonly Color TextPrimary = new(0.95f, 0.96f, 0.94f, 1f);
    internal static readonly Color TextMuted = new(0.68f, 0.71f, 0.69f, 1f);
    internal static readonly Color Error = new(0.94f, 0.52f, 0.42f, 1f);

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
        root.GetComponent<Image>().color = Surface;
        LayoutElement layout = root.GetComponent<LayoutElement>();
        layout.minWidth = 420;
        layout.preferredWidth = 700;
        layout.flexibleWidth = 1;
        layout.minHeight = 52;

        RectTransform viewport = CreateRect("Text Area", root, typeof(RectMask2D));
        Stretch(viewport, 18, 18, 5, 5);

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

    public static TMP_Dropdown CreateDropdown(Transform parent, TMP_FontAsset font)
    {
        RectTransform root = CreateRect("Target", parent, typeof(Image), typeof(TMP_Dropdown), typeof(LayoutElement));
        root.GetComponent<Image>().color = Surface;
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
        template.GetComponent<Image>().color = new Color(0.08f, 0.09f, 0.095f, 1f);

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
        itemBackground.GetComponent<Image>().color = Surface;
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

    public static ItemTile CreateItemTile(Transform parent, TMP_FontAsset font, GameItemRecord record, UnityAction onClick)
    {
        RectTransform root = CreateRect(record.Item.name, parent, typeof(Image), typeof(Button));
        Image background = root.GetComponent<Image>();
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
        return new ItemTile(record, root.gameObject, button);
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
}

internal sealed class ItemTile
{
    public ItemTile(GameItemRecord record, GameObject gameObject, Button button)
    {
        Record = record;
        GameObject = gameObject;
        Button = button;
    }

    public GameItemRecord Record { get; }
    public GameObject GameObject { get; }
    public Button Button { get; }
}
