using System;
using BepInEx.Logging;
using ItemSpawnerEnhanced.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ItemSpawnerEnhanced;

internal sealed class ItemSpawnerController : IDisposable
{
    private readonly ManualLogSource _logger;
    private readonly InputAction _toggleAction;
    private ItemSpawnerWindow? _window;

    public ItemSpawnerController(ModConfig settings, ManualLogSource logger)
    {
        _logger = logger;
        string binding = $"<Keyboard>/{settings.ToggleKey.ToString().ToLowerInvariant()}";
        _toggleAction = new InputAction("ToggleItemSpawner", InputActionType.Button, binding);
        _toggleAction.Enable();
    }

    public void Attach(GUIManager guiManager)
    {
        if (_window != null)
            return;

        try
        {
            _window = ItemSpawnerWindow.Create(guiManager.transform, _logger);
        }
        catch (Exception exception)
        {
            _logger.LogError($"Failed to create item spawner UI: {exception}");
        }
    }

    public void Tick()
    {
        if (_window != null && _toggleAction.WasPressedThisFrame())
            _window.ToggleWindow();
    }

    public void Dispose()
    {
        if (_window != null)
        {
            _window.Shutdown();
            UnityEngine.Object.Destroy(_window.gameObject);
        }
        _window = null;
        _toggleAction.Disable();
        _toggleAction.Dispose();
    }
}
