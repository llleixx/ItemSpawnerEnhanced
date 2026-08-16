using BepInEx.Configuration;
using UnityEngine;

namespace ItemSpawnerEnhanced;

internal sealed class ModConfig
{
    private readonly ConfigEntry<KeyCode> _toggleKey;

    public ModConfig(ConfigFile config)
    {
        _toggleKey = config.Bind(
            "General",
            "ToggleKey",
            KeyCode.F5,
            "The key used to open and close the item spawner.");
    }

    public KeyCode ToggleKey => _toggleKey.Value;
}

