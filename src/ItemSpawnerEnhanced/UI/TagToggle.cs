using ItemSpawnerEnhanced.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ItemSpawnerEnhanced.UI;

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
