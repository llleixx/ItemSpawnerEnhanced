# Item Tags and Catalog Visibility

This document describes how ItemSpawnerEnhanced assigns item tags and which internal PEAK prefabs are hidden from the catalog by default. The current lists are based on PEAK `2.1.a` game assets and the [PEAK Wiki](https://peak.wiki.gg/); they may need to be reviewed after game updates.

## Special useful prefabs kept visible

Some prefabs are unavailable through ordinary loot or are internal variants, but remain useful when deliberately spawned. The following items are therefore **visible by default** and are not part of the hidden list:

| Internal prefab name | Why it remains visible |
| --- | --- |
| `Parachute` | An unused but functional automatic parachute that can prevent fall damage; tagged Consumable + Equipment. |
| `ScoutCookies_Vanilla` | A functional Scout Cookies variant with the older texture. |
| `Warp Compass` | Removed from ordinary loot, but its three-use random-player teleport action still works in multiplayer; tagged Consumable + Mystical. |
| `RescueHook_Infinite` | A functional infinite-use Rescue Claw variant. |

These exceptions are intentionally visible even when `ShowAllItems` is `false`.

## Showing every registered prefab

The default catalog hides confirmed unused, test, cheat, and logically duplicated prefabs. The complete database can be enabled in `BepInEx/config/com.github.lllei.ItemSpawnerEnhanced.cfg`:

```ini
[Catalog]
ShowAllItems = true
```

When enabled, the catalog includes every non-null prefab registered in `ItemDatabase.Instance.Objects`, including all 37 entries in the default-hidden list below. A changed setting is included in catalog cache validation, so the item window rebuilds after the new value is loaded.

## Tag rules

| Tag | Assignment rule |
| --- | --- |
| Food | Assigned through the reviewed vanilla mapping; the native `PackagedFood`, `Berry`, or `Mushroom` flags; or components that consume an item and restore hunger. |
| Consumable | Assigned through the reviewed vanilla mapping, or by `Action_Consume` when the item is neither Food nor Deployable. |
| Equipment | An item that is neither Food nor Deployable and provides a functional benefit without being used primarily as an actively consumed item. Assigned through the reviewed vanilla mapping. |
| Deployable | Assigned through the reviewed vanilla mapping. It may coexist with Mystical, but never with Consumable or Equipment. |
| Mystical | Assigned through the reviewed vanilla mapping or the native `Mystical` item flag. |
| Other | Assigned only when no other static tag was found. |
| Favorite | Added dynamically after the user favorites an item; it is not an intrinsic item classification. |

Food and Consumable are mutually exclusive. Once an item is identified as Food, the Consumable tag is removed. Deployable also removes Consumable and Equipment, so placement actions are not treated as item consumption. Other combinations are intentional: for example, the automatic Parachute is Consumable + Equipment, while Warp Compass is Consumable + Mystical.

## Reviewed vanilla mappings

The names below are English UI names from the game assets. Runtime components and native flags can add tags beyond these explicit mappings, including for some modded items.

### Food

`Airline Food`, `Beehive`, `Big Egg`, `Big Lollipop`, `Black Clusterberry`, `Blue Berrynana`, `Blue Shroomberry`, `Brown Berrynana`, `Bugle Shroom`, `Button Shroom`, `Chubby Shroom`, `Cluster Shroom`, `Coconut`, `Cooked Bird`, `Egg`, `Energy Drink`, `Fortified Milk`, `Frog Legs`, `Gold Prickleberry`, `Granola Bar`, `Green Crispberry`, `Green Kingberry`, `Green Shroomberry`, `Coconut Half` (with `Half-Coconut` retained as a compatibility alias), `Honeycomb`, `Hot Dog`, `Mandrake`, `Marshmallow`, `Medicinal Root`, `Napberry`, `Orange Winterberry`, `Pink Berrynana`, `Purple Kingberry`, `Purple Shroomberry`, `Red Clusterberry`, `Red Crispberry`, `Red Prickleberry`, `Red Shroomberry`, `Scorchberry`, `Scorpion`, `Scout Cookies`, `Small Egg`, `Sports Drink`, `The Early Worm`, `Tick`, `Trail Mix`, `Yellow Berrynana`, `Yellow Clusterberry`, `Yellow Crispberry`, `Yellow Kingberry`, `Yellow Shroomberry`, `Yellow Winterberry`.

### Consumable

`Aloe Vera`, `Antidote`, `Anti-Zooka`, `Balloon`, `Balloon Bunch`, `Bandages`, `Blowgun`, `Bugle of Friendship`, `Candlestick`, `Cure-All`, `Cursed Skull`, `Dynamite`, `Faerie Lantern`, `First Aid Kit`, `Flare`, `Heat Pack`, `Lantern`, `Pandora's Lunchbox`, `Parachute`, `Remedy Fungus`, `Rescue Claw`, `Ritual Dagger`, `Scout Effigy`, `Scoutmaster's Bugle`, `Sunscreen`, `The Book of Bones`, `Torch`, `Warp Compass`, `Warp Fungus`.

### Equipment

`Ancient Idol`, `Backpack`, `Balloon`, `Balloon Bunch`, `Binoculars`, `Bugle`, `Bugle?`, `Compass`, `Fanny Pack` (also stored as `Fannypack` in current assets), `Glider`, `Jetpack`, `Parasol`, `Parachute`, `Pirate's Compass`, `Rocketpack`.

### Deployable

`Anti-Rope Cannon`, `Anti-Rope Spool`, `Bounce Fungus`, `Chain Launcher`, `Checkpoint Flag`, `Cloud Fungus`, `Magic Bean`, `Piton`, `Portable Stove`, `Rope Cannon`, `Rope Spool`, `Scout Cannon`, `Shelf Fungus`.

### Mystical

`Ancient Idol`, `Anti-Rope Cannon`, `Anti-Rope Spool`, `Anti-Zooka`, `Cure-All`, `Cursed Skull`, `Faerie Lantern`, `Pandora's Lunchbox`, `Ritual Dagger`, `Scout Effigy`, `Scout's Ambition`, `Scout's Generosity`, `Scout's Honor`, `Scout's Initiative`, `Scout's Tenacity`, `Scoutmaster's Bugle`, `Strange Gem`, `The Book of Bones`, `Warp Compass`.

### Other

Other has no fixed allowlist. It is assigned only when an item has no reviewed mapping, matching runtime component, or native tag. `Guidebook` is one current example. This fallback also keeps unknown vanilla entries and modded items discoverable without requiring a name entry for every prefab.

## Default-hidden prefabs

The following sections list **all 37** prefabs hidden when `ShowAllItems` is `false`.

### Unused or unavailable through normal gameplay (7)

These entries are documented as unused by the PEAK Wiki or have no ordinary acquisition path in the current assets.

| Internal prefab name | Reason |
| --- | --- |
| `ClimbingChalk` | Unused climbing chalk consumable. |
| `Clusterberry_UNUSED` | Unused green Clusterberry. |
| `FireWood` | Unused stick or campfire-material item. |
| `GuidebookPage_13_FirstTeams` | Unused extra Guidebook page. |
| `Megaphone` | Unused megaphone. |
| `Mushroom Glow` | Unused Weird Shroom with an incomplete glow effect. |
| `Stone` | Unused stone with incomplete campfire-heating behavior. |

### Cheat or progression-skip variants (2)

| Internal prefab name | Reason |
| --- | --- |
| `Cheat Compass` | Developer compass that jumps the run to Caldera. |
| `Cheat Compass 1` | Developer compass that jumps the run to The Kiln. |

### Scene, disguise, or biome duplicate prefabs (7)

These are not necessarily test content, but duplicate a logical item already retained in the catalog.

| Internal prefab name | Retained logical item and reason |
| --- | --- |
| `BingBong_Prop Variant` | Scene prop duplicate; `BingBong` remains visible. |
| `Binoculars_Prop` | Scene prop duplicate; `Binoculars` remains visible. |
| `Bugle_Prop Variant` | Scene prop duplicate; `Bugle` remains visible. |
| `GuidebookPage` | Generic/base page; the actual numbered pages remain visible. |
| `Lollipop_Prop` | Scene prop duplicate; `Lollipop` remains visible. |
| `Mandrake_Hidden` | World disguise state for a Mandrake posing as a Medicinal Root; `MedicinalRoot` and `Mandrake` remain visible. |
| `Parasol_Roots Variant` | Roots loot duplicate with the same icon, meshes, materials, behavior, and display name as `Parasol`; only its loot pool and rarity differ. The main `Parasol` remains visible. |

### Experimental items with no normal spawn pool (3)

| Internal prefab name | Reason |
| --- | --- |
| `ScoutmasterSoul` | Spawn pool is zero and no normal acquisition path was found. |
| `Skull` | Has no `LootData` or usable action and is distinct from the functional `Cursed Skull`. |
| `Warpsketball` | Spawn pool is zero and it contains experimental throw-to-warp behavior. |

### Chess prototypes and placeholder data (18)

The normal black and white chess pieces use prefabs ending in ` B` or ` W` and remain visible. The following intermediate prototypes are hidden. Twelve of them incorrectly reuse Green Kingberry UI data and food components.

| Internal prefab name | Asset state |
| --- | --- |
| `C_Bishop_f Variant` | Intermediate Bishop prototype. |
| `C_Bishop_f` | Incorrectly reuses Green Kingberry data. |
| `C_Bishop_m Variant` | Incorrectly reuses Green Kingberry data. |
| `C_Bishop_m` | Incorrectly reuses Green Kingberry data. |
| `C_King Variant` | Intermediate King prototype. |
| `C_King` | Incorrectly reuses Green Kingberry data. |
| `C_Knight Variant` | Intermediate Knight prototype. |
| `C_Knight` | Incorrectly reuses Green Kingberry data. |
| `C_Pawn_f Variant` | Incorrectly reuses Green Kingberry data. |
| `C_Pawn_f` | Incorrectly reuses Green Kingberry data. |
| `C_Pawn_m Variant` | Intermediate Pawn prototype. |
| `C_Pawn_m` | Incorrectly reuses Green Kingberry data. |
| `C_Queen Variant` | Intermediate Queen prototype. |
| `C_Queen` | Incorrectly reuses Green Kingberry data. |
| `C_Rook_f Variant` | Incorrectly reuses Green Kingberry data. |
| `C_Rook_f` | Incorrectly reuses Green Kingberry data. |
| `C_Rook_m Variant` | Intermediate Rook prototype. |
| `C_Rook_m` | Incorrectly reuses Green Kingberry data. |

## Maintenance notes

- Default hiding uses exact, case-insensitive internal prefab names. It does not remove items through fuzzy keywords.
- `ShowAllItems` controls catalog visibility only; it does not change tags, search, favorites, or spawning behavior.
- Game updates that change internal names, spawn pools, or intended uses should update the mapping, hidden list, this document, and the corresponding tests together.
