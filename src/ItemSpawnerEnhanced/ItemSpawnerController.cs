using System;
using BepInEx.Logging;
using ItemSpawnerEnhanced.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ItemSpawnerEnhanced;

internal sealed class ItemSpawnerController : IDisposable
{
    private readonly ManualLogSource _logger;
    private readonly Key _toggleKey;
    private ItemSpawnerWindow? _window;

    public ItemSpawnerController(ModConfig settings, ManualLogSource logger)
    {
        _logger = logger;
        _toggleKey = settings.ToggleKey;
    }

    public void Attach()
    {
        if (_window != null)
            return;

        try
        {
            _window = ItemSpawnerWindow.Create(_logger);
        }
        catch (Exception exception)
        {
            _logger.LogError($"Failed to create item spawner UI: {exception}");
        }
    }

    public void Tick()
    {
        Keyboard? keyboard = Keyboard.current;
        if (_window != null && keyboard != null && _toggleKey != Key.None && keyboard[_toggleKey].wasPressedThisFrame)
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
    }
}
