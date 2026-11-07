using ItemSpawnerEnhanced.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ItemSpawnerEnhanced.UI;

internal sealed class TagToggle
{
    private readonly Image _background;
    private readonly RawImage? _icon;
    private readonly Image _focusFrame;
    private readonly ControlInteractionState _interaction;
    private bool _active;
    private ItemNameTooltip? _tooltip;
    private ItemNameTooltipTrigger? _tooltipTrigger;
    private string _labelText = string.Empty;

    public TagToggle(
        ItemFilterTag tag,
        Toggle toggle,
        Image background,
        TMP_Text? label,
        RawImage? icon)
    {
        Tag = tag;
        Toggle = toggle;
        _background = background;
        Label = label;
        _icon = icon;
        _focusFrame = RuntimeUiFactory.CreateFocusFrame(toggle.transform);
        _interaction = toggle.gameObject.AddComponent<ControlInteractionState>();
        _interaction.Changed += Render;
        Toggle.transition = Selectable.Transition.None;
        Render();
    }

    public ItemFilterTag Tag { get; }
    public Toggle Toggle { get; }
    public TMP_Text? Label { get; }

    public void SetLabel(string text)
    {
        _labelText = text;
        if (Label != null)
            Label.text = text;
        _tooltipTrigger?.SetText(text);
        _tooltip?.UpdateText(this, text);
    }

    public void ConfigureTooltip(ItemNameTooltip tooltip)
    {
        if (_icon == null)
            return;

        _tooltip = tooltip;
        _tooltipTrigger = Toggle.gameObject.AddComponent<ItemNameTooltipTrigger>();
        _tooltipTrigger.Configure(tooltip, _labelText);
    }

    public void ShowControllerTooltip() =>
        _tooltip?.BeginAnchored(this, _labelText, (RectTransform)Toggle.transform);

    public void SetActive(bool active)
    {
        _active = active;
        Render();
    }

    public void SetFocused(bool focused) => _interaction.SetFocused(focused);

    public void SetControllerPressed(bool pressed) => _interaction.SetControllerPressed(pressed);

    private void Render()
    {
        Core.ControlVisualState state = Core.ControllerVisualState.ForTag(
            _active,
            _interaction.Focused,
            _interaction.Hovered,
            _interaction.Pressed);
        _background.color = RuntimeUiFactory.ResolvePalette(state.Background);
        Color foreground = RuntimeUiFactory.ResolvePalette(state.Foreground);
        if (Label != null)
            Label.color = foreground;
        if (_icon != null)
            _icon.color = foreground;
        _focusFrame.gameObject.SetActive(state.ShowFocus);
    }
}
