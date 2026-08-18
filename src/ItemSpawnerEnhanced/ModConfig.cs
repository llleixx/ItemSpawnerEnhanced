using BepInEx.Configuration;
using ItemSpawnerEnhanced.Core;
using UnityEngine.InputSystem;

namespace ItemSpawnerEnhanced;

internal sealed class ModConfig
{
    private readonly ConfigEntry<Key> _toggleKey;
    private readonly ConfigEntry<bool> _showAllItems;
    private readonly ConfigEntry<bool> _singleTagSelection;
    private readonly ConfigEntry<TagMatchMode> _tagMatchMode;
    private readonly ConfigEntry<string> _favoriteItemNames;

    public ModConfig(ConfigFile config)
    {
        _toggleKey = config.Bind(
            "General",
            "ToggleKey",
            Key.F5,
            "The keyboard key used to open and close the item spawner.");

        _showAllItems = config.Bind(
            "Catalog",
            "ShowAllItems",
            false,
            "Show every item prefab registered by PEAK, including unused, test, cheat, and internal duplicate items.");

        _singleTagSelection = config.Bind(
            "Filtering",
            "SingleTagSelection",
            true,
            "Allow only one selected filter tag at a time. Selecting another tag clears the current selection.");

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
    public bool ShowAllItems => _showAllItems.Value;
    public bool SingleTagSelection => _singleTagSelection.Value;
    public TagMatchMode TagMatchMode => _tagMatchMode.Value;
    public ConfigEntry<bool> SingleTagSelectionEntry => _singleTagSelection;
    public ConfigEntry<TagMatchMode> TagMatchModeEntry => _tagMatchMode;
    public ConfigEntry<string> FavoriteItemNamesEntry => _favoriteItemNames;
}
