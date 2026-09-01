using System;
using BepInEx.Logging;
using ItemSpawnerEnhanced.UI;
using UnityEngine;

namespace ItemSpawnerEnhanced;

internal sealed class ItemSpawnerController : IDisposable
{
    private readonly ManualLogSource _logger;
    private readonly ModConfig _settings;
    private readonly FavoriteStore _favorites;
    private readonly ItemSpawnerInput _input;
    private readonly ItemBrowserSession _browserSession = new();
    private ItemSpawnerWindow? _window;

    public ItemSpawnerController(ModConfig settings, ManualLogSource logger)
    {
        _logger = logger;
        _settings = settings;
        _favorites = new FavoriteStore(settings.FavoriteItemNamesEntry, logger);
        _input = new ItemSpawnerInput(settings, logger);
    }

    public void Attach()
    {
        if (_window != null)
            return;

        try
        {
            _window = ItemSpawnerWindow.Create(_logger, _settings, _favorites, _browserSession);
        }
        catch (Exception exception)
        {
            _logger.LogError($"Failed to create item spawner UI: {exception}");
        }
    }

    public void Tick()
    {
        ItemSpawnerInputSnapshot input = _input.ReadSnapshot();
        if (_window == null)
            return;

        if (input.ControllerToggle)
        {
            if (_window.IsOpen || CanOpenWindow())
                _window.ToggleWindow(WindowInputSource.Controller);
            return;
        }

        if (input.KeyboardToggle)
        {
            if (_window.IsOpen || CanOpenWindow())
                _window.ToggleWindow(WindowInputSource.Keyboard);
            return;
        }

        if (_window.IsOpen)
            _window.HandleControllerInput(input);
    }

    public void Dispose()
    {
        if (_window != null)
        {
            _window.Shutdown();
            UnityEngine.Object.Destroy(_window.gameObject);
        }
        _window = null;
        _input.Dispose();
    }

    private static bool CanOpenWindow()
    {
        GUIManager? gui = GUIManager.instance;
        return Time.timeScale > 0 && gui != null && !gui.windowBlockingInput && !gui.wheelActive;
    }
}
