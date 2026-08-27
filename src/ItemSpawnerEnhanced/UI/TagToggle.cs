using ItemSpawnerEnhanced.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ItemSpawnerEnhanced.UI;

internal sealed class TagToggle
{
    private readonly Image _background;
    private readonly RawImage? _heart;
    private readonly Image _focusFrame;
    private readonly ControlInteractionState _interaction;
    private bool _active;

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
        _focusFrame = RuntimeUiFactory.CreateFocusFrame(toggle.transform);
        _interaction = toggle.gameObject.AddComponent<ControlInteractionState>();
        _interaction.Changed += Render;
        Toggle.transition = Selectable.Transition.None;
        Render();
    }

    public ItemFilterTag Tag { get; }
    public Toggle Toggle { get; }
    public TMP_Text Label { get; }

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
        Label.color = foreground;
        if (_heart != null)
            _heart.color = foreground;
        _focusFrame.gameObject.SetActive(state.ShowFocus);
    }
}
