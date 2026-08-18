# Changelog

## Unreleased

- Kept favorited default-hidden prefabs visible when `ShowAllItems` is disabled.

## 1.2.0 - 2026-08-18

- Added continuous item spawning while the left mouse button is held on an item tile.
- Added a default-on `SingleTagSelection` option that replaces the selected tag when another tag is chosen.
- Preloaded the item catalog, search index, and browser UI in the background to reduce the delay when opening the item spawner for the first time.
- Made reviewed vanilla tag mappings use exact prefab names, fixing missing Consumable tags for Anti-Zooka, The Book of Bones, and Ritual Dagger.
- Changed toggle-key handling to support direct key capture and immediate runtime rebinding through PEAKLib ModConfig without restarting the game.

## 1.1.0 - 2026-08-17

- Added localized multi-select tags for Food, Consumables, Equipment, Deployables, Mystical items, Other items, and Favorites.
- Added configurable AND/OR tag matching, defaulting to AND.
- Added persistent right-click favorites with heart markers on item tiles.
- Added one-click controls for clearing the search query or all selected tags.
- Rounded the window, controls, tag buttons, item tiles, and tooltips.
- Updated the package icon.
- Fixed food detection for items such as Coconut Half and made Food mutually exclusive with Consumables.
- Corrected the Food, Consumable, Equipment, and Other assignments for several vanilla items.
- Hid known unused, test, cheat, scene-prop, and internal duplicate item prefabs from the catalog.
- Added a `ShowAllItems` catalog option for revealing every registered item prefab.
- Improved UI lifecycle cleanup and reorganized browser, layout, control, and runtime-asset responsibilities.

## 1.0.1 - 2026-08-17

- Added hover tooltips that show complete localized item names.
- Expanded the README gallery with optimized WebP screenshots for six languages.
- Clarified manual installation and language search extension documentation.

## 1.0.0 - 2026-08-16

- Runtime-built item spawner UI with a larger centered search field.
- Smart spectator targeting and an explicit player target dropdown.
- Current-language item names with English and internal-name fallbacks.
- Ranked Unicode search with substring and bounded typo matching.
- Simplified and Traditional Chinese pinyin and initial matching.
- Public search alias provider API for language extension mods.
- UI translations for all 15 languages currently supported by PEAK.
