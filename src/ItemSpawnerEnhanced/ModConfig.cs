using BepInEx.Configuration;
using ItemSpawnerEnhanced.Core;
using UnityEngine.InputSystem;

namespace ItemSpawnerEnhanced;

internal sealed class ModConfig
{
    private readonly ConfigEntry<Key> _toggleKey;
    private readonly ConfigEntry<TagMatchMode> _tagMatchMode;
    private readonly ConfigEntry<string> _favoriteItemNames;

    public ModConfig(ConfigFile config)
    {
        _toggleKey = config.Bind(
            "General",
            "ToggleKey",
            Key.F5,
            "The keyboard key used to open and close the item spawner.");

        _tagMatchMode = config.Bind(
            "Filtering",
            "TagMatchMode",
            TagMatchMode.And,
            "How selected tags are combined. And requires every selected tag; Or requires any selected tag.");

        _favoriteItemNames = config.Bind(
            "Favorites",
            "ItemNames",
            "[]",
            "Favorite item prefab names stored as a JSON array. Manage these in the item spawner UI.");
    }

    public Key ToggleKey => _toggleKey.Value;
    public TagMatchMode TagMatchMode => _tagMatchMode.Value;
    public ConfigEntry<TagMatchMode> TagMatchModeEntry => _tagMatchMode;
    public ConfigEntry<string> FavoriteItemNamesEntry => _favoriteItemNames;
}
