using BepInEx.Configuration;
using ItemSpawnerEnhanced.Core;

namespace ItemSpawnerEnhanced;

internal sealed class ModConfig
{
    private readonly ConfigEntry<bool> _showAllItems;
    private readonly ConfigEntry<bool> _singleTagSelection;
    private readonly ConfigEntry<bool> _clearTagsOnSearch;
    private readonly ConfigEntry<TagMatchMode> _tagMatchMode;
    private readonly ConfigEntry<string> _favoriteItemNames;

    public ModConfig(ConfigFile config)
    {
        KeyboardTogglePath = config.Bind(
            "Controls",
            "Keyboard Toggle Path",
            "<Keyboard>/f5",
            "Unity Input System control path used to open and close the item spawner with a keyboard. " +
            "Leave empty to disable.");

        ControllerChordModifierPath = config.Bind(
            "Controls",
            "Controller Chord Modifier Path",
            "<Gamepad>/select",
            "Controller button held as the optional modifier for the item spawner shortcut. " +
            "Default: <Gamepad>/select (Xbox View / PlayStation Share). " +
            "Leave empty for a single-button shortcut.");

        ControllerTogglePath = config.Bind(
            "Controls",
            "Controller Toggle Path",
            "<Gamepad>/rightTrigger",
            "Controller button pressed with the optional modifier to open or close the item spawner. " +
            "Default: <Gamepad>/rightTrigger (Xbox RT / PlayStation R2). " +
            "Leave empty to disable.");

        _showAllItems = config.Bind(
            "General",
            "Show All Items",
            false,
            "Show every item prefab registered by PEAK, including unused, test, cheat, and internal duplicate items.");

        _singleTagSelection = config.Bind(
            "General",
            "Single Tag Selection",
            true,
            "Allow only one selected filter tag at a time. Selecting another tag clears the current selection.");

        _clearTagsOnSearch = config.Bind(
            "General",
            "Clear Tags On Search",
            true,
            "Clear selected filter tags when entering or editing a non-empty search query. ");

        _tagMatchMode = config.Bind(
            "General",
            "Tag Match Mode",
            TagMatchMode.And,
            "How selected tags are combined. And requires every selected tag; Or requires any selected tag.");

        _favoriteItemNames = config.Bind(
            "Favorites",
            "ItemNames",
            "[]",
            "Favorite item prefab names stored as a JSON array. Manage these in the item spawner UI.");
    }

    public ConfigEntry<string> KeyboardTogglePath { get; }
    public ConfigEntry<string> ControllerChordModifierPath { get; }
    public ConfigEntry<string> ControllerTogglePath { get; }
    public bool ShowAllItems => _showAllItems.Value;
    public bool SingleTagSelection => _singleTagSelection.Value;
    public bool ClearTagsOnSearch => _clearTagsOnSearch.Value;
    public TagMatchMode TagMatchMode => _tagMatchMode.Value;
    public ConfigEntry<bool> SingleTagSelectionEntry => _singleTagSelection;
    public ConfigEntry<TagMatchMode> TagMatchModeEntry => _tagMatchMode;
    public ConfigEntry<string> FavoriteItemNamesEntry => _favoriteItemNames;

}
