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
    private InputAction? _keyboardToggleAction;
    private InputAction? _controllerToggleAction;

    public ItemSpawnerInput(ModConfig config, ManualLogSource logger)
    {
        _config = config;
        _logger = logger;
        BindToggleActions();
        _config.KeyboardTogglePath.SettingChanged += OnTogglePathChanged;
        _config.ControllerChordModifierPath.SettingChanged += OnTogglePathChanged;
        _config.ControllerTogglePath.SettingChanged += OnTogglePathChanged;
    }

    public ItemSpawnerInputSnapshot ReadSnapshot()
    {
        Keyboard? keyboard = Keyboard.current;
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

        return new ItemSpawnerInputSnapshot(
            _keyboardToggleAction?.WasPressedThisFrame() == true,
            _controllerToggleAction?.WasPressedThisFrame() == true,
            navigation,
            gamepad?.buttonSouth.wasPressedThisFrame == true,
            gamepad?.buttonSouth.isPressed == true,
            gamepad?.buttonSouth.wasReleasedThisFrame == true,
            keyboard?.escapeKey.wasPressedThisFrame == true || gamepad?.buttonEast.wasPressedThisFrame == true,
            gamepad?.buttonWest.wasPressedThisFrame == true,
            gamepad?.leftShoulder.wasPressedThisFrame == true,
            gamepad?.rightShoulder.wasPressedThisFrame == true,
            mouseActivity);
    }

    public void Dispose()
    {
        _config.KeyboardTogglePath.SettingChanged -= OnTogglePathChanged;
        _config.ControllerChordModifierPath.SettingChanged -= OnTogglePathChanged;
        _config.ControllerTogglePath.SettingChanged -= OnTogglePathChanged;
        DisposeActions();
    }

    private void OnTogglePathChanged(object sender, EventArgs eventArgs)
    {
        BindToggleActions();
    }

    private void BindToggleActions()
    {
        DisposeActions();

        string keyboardPath = InputBindingRules.NormalizeBindingPath(
            _config.KeyboardTogglePath.Value);
        string modifierPath = InputBindingRules.NormalizeBindingPath(
            _config.ControllerChordModifierPath.Value);
        string controllerPath = InputBindingRules.NormalizeBindingPath(
            _config.ControllerTogglePath.Value);

        if (keyboardPath.Length > 0)
            _keyboardToggleAction = CreateKeyboardToggleAction(keyboardPath);
        if (controllerPath.Length > 0)
            _controllerToggleAction = CreateControllerToggleAction(modifierPath, controllerPath);
    }

    private InputAction? CreateKeyboardToggleAction(string path)
    {
        InputAction? action = null;
        try
        {
            action = new InputAction("ItemSpawnerEnhanced Keyboard Toggle", InputActionType.Button, path);
            action.Enable();
            return action;
        }
        catch (Exception exception)
        {
            action?.Dispose();
            _logger.LogWarning(
                $"Keyboard window binding is invalid and has been disabled: {exception.Message}");
            return null;
        }
    }

    private InputAction? CreateControllerToggleAction(string modifierPath, string togglePath)
    {
        InputAction? action = null;
        try
        {
            action = new InputAction(
                "ItemSpawnerEnhanced Controller Toggle",
                InputActionType.Button);
            if (modifierPath.Length == 0)
            {
                action.AddBinding(togglePath);
            }
            else
            {
                action.AddCompositeBinding("OneModifier")
                    .With("modifier", modifierPath)
                    .With("binding", togglePath);
            }

            action.Enable();
            return action;
        }
        catch (Exception exception)
        {
            action?.Dispose();
            _logger.LogWarning(
                $"Controller window binding is invalid and has been disabled: {exception.Message}");
            return null;
        }
    }

    private void DisposeActions()
    {
        _keyboardToggleAction?.Dispose();
        _keyboardToggleAction = null;

        _controllerToggleAction?.Dispose();
        _controllerToggleAction = null;
    }
}
