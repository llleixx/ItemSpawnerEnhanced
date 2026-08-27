# ItemSpawnerEnhanced

[![GitHub Repo](https://img.shields.io/badge/GitHub-llleixx%2FItemSpawnerEnhanced-black?logo=github)](https://github.com/llleixx/ItemSpawnerEnhanced)
[![Thunderstore Downloads](https://img.shields.io/thunderstore/dt/lllei/ItemSpawnerEnhanced?logo=thunderstore&label=Downloads)](https://thunderstore.io/c/peak/p/lllei/ItemSpawnerEnhanced/)

A client-side item spawner for PEAK built for players across **every supported game language**, with **localized smart search**, **spectator-aware targeting**, and support for spawning items for **any connected Scout**.

| Favorites | Combined tag filtering |
| :---: | :---: |
| ![Favorites filter showing favorited item tiles](https://raw.githubusercontent.com/llleixx/ItemSpawnerEnhanced/main/docs/media/favorites.webp) | ![Consumables and Mystical tags selected together](https://raw.githubusercontent.com/llleixx/ItemSpawnerEnhanced/main/docs/media/tag-filtering.webp) |

| English (`pack`) | 简体中文拼音首字母 (`bb`) |
| :---: | :---: |
| ![English pack search](https://raw.githubusercontent.com/llleixx/ItemSpawnerEnhanced/main/docs/media/search-en-pack.webp) | ![Simplified Chinese pinyin initials search](https://raw.githubusercontent.com/llleixx/ItemSpawnerEnhanced/main/docs/media/search-zh-hans-pinyin-initials.webp) |

| 日本語 (`パック`) | Русский (`ранец`) |
| :---: | :---: |
| ![Japanese pack search](https://raw.githubusercontent.com/llleixx/ItemSpawnerEnhanced/main/docs/media/search-ja-pakku.webp) | ![Russian ranets search with a full-name tooltip](https://raw.githubusercontent.com/llleixx/ItemSpawnerEnhanced/main/docs/media/search-ru-ranets.webp) |

| Português (BR) (`mochila`) | Español (España) (`mochila`) |
| :---: | :---: |
| ![Brazilian Portuguese mochila search with a full-name tooltip](https://raw.githubusercontent.com/llleixx/ItemSpawnerEnhanced/main/docs/media/search-pt-br-mochila.webp) | ![Spanish Spain mochila search](https://raw.githubusercontent.com/llleixx/ItemSpawnerEnhanced/main/docs/media/search-es-es-mochila.webp) |

## Features

### Core features

- **Responsive item browser:** Press `F5` or hold `View/Share` and press `RT/R2` to open a clean interface with larger text and controls.
- **Full controller support:** Navigate targets, filters, and items with a gamepad, with persistent focus, automatic scrolling, controller favorites, and accessible focus visuals.
- **Smart targeting:** Spawn for yourself while alive or automatically target the Scout you are spectating after death.
- **Any Scout:** Select any available Scout in the room from the target dropdown.
- **Modded item support:** Discover items registered in PEAK's item database, including items added by other mods.
- **Tag filtering:** Filter by Food, Consumables, Equipment, Deployables, Mystical, Other, or Favorites. Tags are single-select by default, with optional multi-select AND/OR matching.
- **Curated catalog:** Hide known unused and internal duplicate prefabs by default, with an option to reveal every registered item.
- **Persistent favorites:** Right-click an item to favorite it, mark its tile with a heart, and filter favorites like any other tag.
- **Fully localized UI:** Use interface text for every language currently supported by PEAK.
- **Native multiplayer spawning:** Spawn through PEAK's built-in network RPC; click for one item or hold the left mouse button to keep spawning.

### Search available in every language

- **Multiple name sources:** Search by the current-language name, English name, internal/prefab name, or unlocalized source name.
- **Flexible matching:** Find results by exact match, prefix, word prefix, or substring.
- **Typo tolerance:** Find longer queries even with a small typing mistake.
- **Relevant ranking:** See matches for the displayed name ahead of fallback aliases.

### Language-specific search

| Language | Implementation | Features |
| --- | --- | --- |
| 简体中文 | **随主 Mod 提供：**[`ItemSpawnerEnhanced.ChineseSearch.dll`](https://github.com/llleixx/ItemSpawnerEnhanced/tree/main/src/ItemSpawnerEnhanced.ChineseSearch)，借助 [`TinyPinyin`](https://github.com/hstarorg/TinyPinyin.Net) 实现 | 支持**空格全拼**（如 `sheng suo qiang`）、**连续全拼**（如 `shengsuoqiang`）和**拼音首字母**（如 `ssq`）搜索。 |
| 繁體中文 | **隨主 Mod 提供：**[`ItemSpawnerEnhanced.ChineseSearch.dll`](https://github.com/llleixx/ItemSpawnerEnhanced/tree/main/src/ItemSpawnerEnhanced.ChineseSearch)，借助 [`TinyPinyin`](https://github.com/hstarorg/TinyPinyin.Net) 實現 | 支援**空格全拼**（如 `sheng suo qiang`）、**連續全拼**（如 `shengsuoqiang`）和**拼音首字母**（如 `ssq`）搜尋。 |

## Why ItemSpawnerEnhanced?

The comparison below uses [**quackandcheese-ItemSpawner 0.1.4**](https://thunderstore.io/c/peak/p/quackandcheese/ItemSpawner/v/0.1.4/) as the baseline.

| Area | ItemSpawnerEnhanced | [quackandcheese-ItemSpawner 0.1.4](https://thunderstore.io/c/peak/p/quackandcheese/ItemSpawner/v/0.1.4/) |
| --- | --- | --- |
| **External installation requirements** | `BepInExPack_PEAK` only; language extensions include their own runtime code | `BepInExPack_PEAK` and `AutoHookGenPatcher` |
| **Item names** | Uses PEAK's current-language names, with English and internal names as search fallbacks | Displays the item's source UI name |
| **Search sources** | Current-language, English, internal/prefab, unlocalized, and registered language aliases | Displayed item text only |
| **Match behavior** | Exact, prefix, word-prefix, substring, and bounded typo matching, with ranked results | Case-insensitive prefix matching |
| **Spawn target** | Smart Target or any available Scout in the room | Local Scout only |
| **Interface** | Larger text and controls designed for browsing and target selection | Original compact item browser |
| **Language search extensions** | Public alias-provider API; providers can be contributed here or released as separate mods | No dedicated language-alias provider API |
| **Items added by other mods** | Supported | Supported |

## Language Search Extensions

Language mod authors are welcome to add transliterations and other language-specific search aliases. A provider can be submitted to the [ItemSpawnerEnhanced repository](https://github.com/llleixx/ItemSpawnerEnhanced) or distributed as a standalone companion mod. See the [language search extension guide](https://github.com/llleixx/ItemSpawnerEnhanced/blob/main/docs/LANGUAGE_SEARCH_EXTENSIONS.md) for both workflows and a TinyPinyin-based example.

## Installation

Install ItemSpawnerEnhanced with a mod manager. For a manual installation:

1. Install `BepInExPack_PEAK`.
2. Extract the package and copy `plugins/ItemSpawnerEnhanced` into `<PEAK>/BepInEx/plugins/`.

The installed files should be:

```text
<PEAK>/BepInEx/plugins/ItemSpawnerEnhanced/ItemSpawnerEnhanced.dll
<PEAK>/BepInEx/plugins/ItemSpawnerEnhanced/ItemSpawnerEnhanced.ChineseSearch.dll
```

## Usage

Press `F5`, or hold `View/Share` and press `RT/R2`, to open the item spawner.

- Click an item to spawn it, or hold the left mouse button to spawn repeatedly.
- Right-click an item to favorite or unfavorite it.
- Keyboard opening focuses search; controller opening focuses the first visible item.

### Controller controls

| Input | Action |
| --- | --- |
| Left stick / D-pad | Move focus between the target, tags, clear button, and item grid |
| A / Cross | Activate the focused control or spawn the focused item; hold to spawn repeatedly |
| B / Circle | Close the target list, or close the item spawner |
| X / Square | Favorite or unfavorite the focused item |
| LB / L1 and RB / R1 | Cycle backward or forward through the target, tag, and item regions |

Search entry is currently a mouse-and-keyboard workflow.

## Configuration

Settings are stored in `BepInEx/config/com.github.lllei.ItemSpawnerEnhanced.cfg`.

| Setting | Default | Purpose |
| --- | --- | --- |
| `General.ToggleKey` | `F5` | Keyboard shortcut for opening and closing the item spawner |
| `Controls.ControllerChordModifierPath` | `<Gamepad>/select` | Controller button held as the shortcut modifier |
| `Controls.ControllerTogglePath` | `<Gamepad>/rightTrigger` | Controller button pressed to open or close the item spawner |
| `Filtering.SingleTagSelection` | `true` | Replace the active tag when another tag is selected |
| `Filtering.TagMatchMode` | `And` | Combine multiple selected tags with `And` or `Or` matching |
| `Catalog.ShowAllItems` | `false` | Include unused, test, cheat, and internal duplicate prefabs |

Controller bindings use Unity Input System paths. Set the modifier to empty or `None` for a single-button shortcut; set the toggle path to empty or `None` to disable controller opening. Favorites are managed by the UI and saved automatically in the same configuration file.

See the [item tag and catalog visibility reference](https://github.com/llleixx/ItemSpawnerEnhanced/blob/main/docs/ITEM_TAGS.md) for classification rules and the complete default-hidden list.

## Multiplayer behavior

PEAK's built-in spawn RPC is used. A client with this mod can request an item for **any connected Scout available to the game**, and the room's **master client** performs the spawn. This is intentionally unrestricted and is **not a host-enforced permission system**. Click an item once to create one, or hold the left mouse button on its tile to spawn repeatedly.

## Building

Copy `PeakGameDir.props.example` to `PeakGameDir.props`, set the PEAK path, then run:

```powershell
dotnet test .\tests\ItemSpawnerEnhanced.Core.Tests\ItemSpawnerEnhanced.Core.Tests.csproj -c Release
dotnet test .\tests\ItemSpawnerEnhanced.ChineseSearch.Tests\ItemSpawnerEnhanced.ChineseSearch.Tests.csproj -c Release
dotnet build .\src\ItemSpawnerEnhanced.ChineseSearch\ItemSpawnerEnhanced.ChineseSearch.csproj -c Release
powershell -NoProfile -ExecutionPolicy Bypass -File .\build\Deploy.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\build\Package.ps1
```

Optimize a 16:10 screenshot for the README with:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\build\Optimize-Screenshot.ps1 -InputPath .\screenshot.jpg -OutputPath .\docs\media\search-en-pack.webp
```

## Acknowledgements

Special thanks to my friend **「饺子」** for drawing the mod icon, and to the [PEAK Wiki community](https://peak.wiki.gg/) for documenting the game's items and mechanics.
