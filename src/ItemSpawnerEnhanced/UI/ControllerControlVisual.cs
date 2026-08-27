using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ItemSpawnerEnhanced.UI;

internal sealed class ControlInteractionState : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler
{
    public event Action? Changed;

    public bool Focused { get; private set; }
    public bool Hovered { get; private set; }
    public bool Pressed { get; private set; }

    public void SetFocused(bool focused)
    {
        if (Focused == focused)
            return;
        Focused = focused;
        Changed?.Invoke();
    }

    public void SetControllerPressed(bool pressed)
    {
        if (Pressed == pressed)
            return;
        Pressed = pressed;
        Changed?.Invoke();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Hovered = true;
        Changed?.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Hovered = false;
        Pressed = false;
        Changed?.Invoke();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;
        Pressed = true;
        Changed?.Invoke();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;
        Pressed = false;
        Changed?.Invoke();
        if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == gameObject)
            EventSystem.current.SetSelectedGameObject(null);
    }

    private void OnDisable()
    {
        Focused = false;
        Hovered = false;
        Pressed = false;
        Changed?.Invoke();
    }
}

internal sealed class ActionButtonVisual
{
    private readonly Button _button;
    private readonly Image _background;
    private readonly Graphic _foreground;
    private readonly Image _focusFrame;
    private readonly ControlInteractionState _interaction;
    private bool _enabled = true;

    public ActionButtonVisual(Button button, Image background, Graphic foreground)
    {
        _button = button;
        _background = background;
        _foreground = foreground;
        _focusFrame = RuntimeUiFactory.CreateFocusFrame(button.transform);
        _interaction = button.gameObject.AddComponent<ControlInteractionState>();
        _interaction.Changed += Render;
        _button.transition = Selectable.Transition.None;
        Render();
    }

    public Button Button => _button;
    public bool Enabled => _enabled;

    public void SetEnabled(bool enabled)
    {
        _enabled = enabled;
        _button.interactable = enabled;
        Render();
    }

    public void SetFocused(bool focused) => _interaction.SetFocused(focused && _enabled);

    public void SetControllerPressed(bool pressed) =>
        _interaction.SetControllerPressed(pressed && _enabled);

    private void Render()
    {
        Core.ControlVisualState state = Core.ControllerVisualState.ForAction(
            _enabled,
            _interaction.Focused,
            _interaction.Hovered,
            _interaction.Pressed);
        _background.color = RuntimeUiFactory.ResolvePalette(state.Background);
        _foreground.color = _enabled
            ? RuntimeUiFactory.ResolvePalette(state.Foreground)
            : new Color(
                RuntimeUiFactory.TextMuted.r,
                RuntimeUiFactory.TextMuted.g,
                RuntimeUiFactory.TextMuted.b,
                0.35f);
        _focusFrame.gameObject.SetActive(state.ShowFocus);
    }
}
