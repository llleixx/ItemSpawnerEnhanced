using System;
using BepInEx.Logging;
using ItemSpawnerEnhanced.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ItemSpawnerEnhanced;

internal readonly struct ItemSpawnerInputSnapshot
{
    public ItemSpawnerInputSnapshot(
        bool keyboardToggle,
        bool controllerToggle,
        Vector2 navigation,
        bool confirmPressed,
        bool confirmHeld,
        bool confirmReleased,
        bool cancelPressed,
        bool favoritePressed,
        bool previousTargetPressed,
        bool nextTargetPressed,
        bool mouseActivity)
    {
        KeyboardToggle = keyboardToggle;
        ControllerToggle = controllerToggle;
        Navigation = navigation;
        ConfirmPressed = confirmPressed;
        ConfirmHeld = confirmHeld;
        ConfirmReleased = confirmReleased;
        CancelPressed = cancelPressed;
        FavoritePressed = favoritePressed;
        PreviousTargetPressed = previousTargetPressed;
        NextTargetPressed = nextTargetPressed;
        MouseActivity = mouseActivity;
    }

    public bool KeyboardToggle { get; }
    public bool ControllerToggle { get; }
    public Vector2 Navigation { get; }
    public bool ConfirmPressed { get; }
    public bool ConfirmHeld { get; }
    public bool ConfirmReleased { get; }
    public bool CancelPressed { get; }
    public bool FavoritePressed { get; }
    public bool PreviousTargetPressed { get; }
    public bool NextTargetPressed { get; }
    public bool MouseActivity { get; }

    public bool ControllerActivity =>
        ControllerToggle || Navigation.sqrMagnitude >= 0.25f || ConfirmPressed || CancelPressed ||
        FavoritePressed || PreviousTargetPressed || NextTargetPressed;
}

internal sealed class ItemSpawnerInput : IDisposable
{
    private readonly ModConfig _config;
    private readonly ManualLogSource _logger;
    private InputAction? _controllerToggleAction;
    private string _bindingSignature = string.Empty;

    public ItemSpawnerInput(ModConfig config, ManualLogSource logger)
    {
        _config = config;
        _logger = logger;
    }

    public ItemSpawnerInputSnapshot ReadSnapshot()
    {
        EnsureActions();
        Gamepad? gamepad = Gamepad.current;
        Mouse? mouse = Mouse.current;
        Vector2 navigation = gamepad == null
            ? Vector2.zero
            : Vector2.ClampMagnitude(gamepad.leftStick.ReadValue() + gamepad.dpad.ReadValue(), 1f);
        bool mouseActivity = mouse != null &&
                             (mouse.delta.ReadValue().sqrMagnitude > 0.25f ||
                              mouse.leftButton.wasPressedThisFrame ||
                              mouse.rightButton.wasPressedThisFrame ||
                              mouse.middleButton.wasPressedThisFrame ||
                              mouse.scroll.ReadValue().sqrMagnitude > 0.01f);

        KeyCode toggleKey = _config.ToggleKey;
        return new ItemSpawnerInputSnapshot(
            toggleKey != KeyCode.None && Input.GetKeyDown(toggleKey),
            _controllerToggleAction?.WasPressedThisFrame() == true,
            navigation,
            gamepad?.buttonSouth.wasPressedThisFrame == true,
            gamepad?.buttonSouth.isPressed == true,
            gamepad?.buttonSouth.wasReleasedThisFrame == true,
            Input.GetKeyDown(KeyCode.Escape) || gamepad?.buttonEast.wasPressedThisFrame == true,
            gamepad?.buttonWest.wasPressedThisFrame == true,
            gamepad?.leftShoulder.wasPressedThisFrame == true,
            gamepad?.rightShoulder.wasPressedThisFrame == true,
            mouseActivity);
    }

    public void Dispose()
    {
        DisposeAction();
        _bindingSignature = string.Empty;
    }

    private void EnsureActions()
    {
        string modifierPath = ControllerBindingRules.NormalizeBindingPath(
            _config.ControllerChordModifierPath.Value);
        string togglePath = ControllerBindingRules.NormalizeBindingPath(
            _config.ControllerTogglePath.Value);
        string signature = $"{modifierPath}\n{togglePath}";
        if (signature == _bindingSignature)
            return;

        DisposeAction();
        _bindingSignature = signature;
        if (togglePath.Length == 0)
            return;

        try
        {
            _controllerToggleAction = new InputAction(
                "ItemSpawnerEnhanced Controller Toggle",
                InputActionType.Button);
            if (modifierPath.Length == 0)
            {
                _controllerToggleAction.AddBinding(togglePath);
            }
            else
            {
                _controllerToggleAction.AddCompositeBinding("OneModifier")
                    .With("modifier", modifierPath)
                    .With("binding", togglePath);
            }

            _controllerToggleAction.Enable();
        }
        catch (Exception exception)
        {
            DisposeAction();
            _logger.LogWarning(
                $"Controller window binding is invalid and has been disabled: {exception.Message}");
        }
    }

    private void DisposeAction()
    {
        if (_controllerToggleAction == null)
            return;

        _controllerToggleAction.Disable();
        _controllerToggleAction.Dispose();
        _controllerToggleAction = null;
    }
}
