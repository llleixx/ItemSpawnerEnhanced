using BepInEx.Configuration;
using UnityEngine.InputSystem;

namespace ItemSpawnerEnhanced;

internal sealed class ModConfig
{
    private readonly ConfigEntry<Key> _toggleKey;

    public ModConfig(ConfigFile config)
    {
        _toggleKey = config.Bind(
            "General",
            "ToggleKey",
            Key.F5,
            "The keyboard key used to open and close the item spawner.");
    }

    public Key ToggleKey => _toggleKey.Value;
}
