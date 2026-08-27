using System.Collections.Generic;
using System.Linq;
using ItemSpawnerEnhanced.Core;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ItemSpawnerEnhanced.UI;

internal sealed class TargetDropdownVisual
{
    private readonly Image _background;
    private readonly TMP_Text _caption;
    private readonly TMP_Text _arrow;
    private readonly Image _focusFrame;
    private readonly ControlInteractionState _interaction;
    private bool _enabled = true;
    private bool _expanded;

    public TargetDropdownVisual(
        TMP_Dropdown dropdown,
        Image background,
        TMP_Text caption,
        TMP_Text arrow)
    {
        Dropdown = dropdown;
        _background = background;
        _caption = caption;
        _arrow = arrow;
        _focusFrame = RuntimeUiFactory.CreateFocusFrame(dropdown.transform);
        _interaction = dropdown.gameObject.AddComponent<ControlInteractionState>();
        _interaction.Changed += Render;
        dropdown.transition = Selectable.Transition.None;
        Render();
    }

    public TMP_Dropdown Dropdown { get; }
    public bool Expanded => _expanded;
    public bool Enabled => _enabled;

    public void SetFocused(bool focused) => _interaction.SetFocused(focused && _enabled && !_expanded);

    public void SetExpanded(bool expanded)
    {
        if (_expanded == expanded)
            return;
        _expanded = expanded;
        Render();
    }

    public void SetEnabled(bool enabled)
    {
        _enabled = enabled;
        Dropdown.interactable = enabled;
        if (!enabled)
            _interaction.SetFocused(false);
        Render();
    }

    public IReadOnlyList<DropdownOptionVisual> GetVisibleOptions() =>
        Dropdown.GetComponentsInChildren<DropdownOptionVisual>(includeInactive: false)
            .Where(option => option.gameObject.activeInHierarchy)
            .OrderBy(option => option.transform.GetSiblingIndex())
            .ToArray();

    private void Render()
    {
        ControlVisualState state = ControllerVisualState.ForAction(
            _enabled,
            _interaction.Focused && !_expanded,
            _interaction.Hovered,
            _interaction.Pressed);
        _background.color = RuntimeUiFactory.ResolvePalette(state.Background);
        _caption.color = RuntimeUiFactory.ResolvePalette(state.Foreground);
        _arrow.color = _enabled ? RuntimeUiFactory.Accent : RuntimeUiFactory.TextMuted;
        _arrow.text = _expanded ? "^" : "v";
        _focusFrame.gameObject.SetActive(state.ShowFocus && !_expanded);
    }
}

internal sealed class DropdownOptionVisual : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler
{
    [SerializeField] private Image _background = null!;
    [SerializeField] private TMP_Text _label = null!;
    [SerializeField] private Toggle _toggle = null!;
    [SerializeField] private Image _focusFrame = null!;

    private bool _focused;
    private bool _hovered;
    private bool _pressed;
    private bool _lastCurrent;

    public void Configure(Image background, TMP_Text label, Toggle toggle)
    {
        _background = background;
        _label = label;
        _toggle = toggle;
        _focusFrame = RuntimeUiFactory.CreateFocusFrame(
            transform,
            outerInset: 2f,
            separatorInset: 3f);
        _toggle.transition = Selectable.Transition.None;
        Navigation navigation = _toggle.navigation;
        navigation.mode = Navigation.Mode.None;
        _toggle.navigation = navigation;
        Render();
    }

    public void SetFocused(bool focused)
    {
        if (_focused == focused)
            return;
        _focused = focused;
        Render();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _hovered = true;
        Render();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _hovered = false;
        _pressed = false;
        Render();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;
        _pressed = true;
        Render();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;
        _pressed = false;
        Render();
    }

    private void OnEnable()
    {
        _focused = false;
        _hovered = false;
        _pressed = false;
        _lastCurrent = _toggle != null && _toggle.isOn;
        Render();
    }

    private void Update()
    {
        bool current = _toggle != null && _toggle.isOn;
        if (current == _lastCurrent)
            return;
        _lastCurrent = current;
        Render();
    }

    private void OnDisable()
    {
        _focused = false;
        _hovered = false;
        _pressed = false;
    }

    private void Render()
    {
        if (_background == null || _label == null || _toggle == null || _focusFrame == null)
            return;

        ControlVisualState state = ControllerVisualState.ForTargetOption(
            _toggle.isOn,
            _focused,
            _hovered || _pressed);
        _background.color = RuntimeUiFactory.ResolvePalette(state.Background);
        _label.color = RuntimeUiFactory.ResolvePalette(state.Foreground);
        _focusFrame.gameObject.SetActive(state.ShowFocus);
    }
}
