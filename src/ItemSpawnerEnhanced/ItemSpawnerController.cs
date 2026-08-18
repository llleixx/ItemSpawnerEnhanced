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
    private readonly ItemBrowserSession _browserSession = new();
    private ItemSpawnerWindow? _window;

    public ItemSpawnerController(ModConfig settings, ManualLogSource logger)
    {
        _logger = logger;
        _settings = settings;
        _favorites = new FavoriteStore(settings.FavoriteItemNamesEntry, logger);
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
        KeyCode toggleKey = _settings.ToggleKey;
        if (_window != null && toggleKey != KeyCode.None && Input.GetKeyDown(toggleKey))
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
