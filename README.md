# ItemSpawnerEnhanced

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

- **Responsive item browser:** Press `F5` to open a clean interface with larger text and controls.
- **Smart targeting:** Spawn for yourself while alive or automatically target the Scout you are spectating after death.
- **Any Scout:** Select any available Scout in the room from the target dropdown.
- **Modded item support:** Discover items registered in PEAK's item database, including items added by other mods.
- **Tag filtering:** Combine Food, Consumables, Equipment, Deployables, Mystical, and Other tags using configurable AND or OR matching.
- **Curated catalog:** Hide known unused and internal duplicate prefabs by default, with an option to reveal every registered item.
- **Persistent favorites:** Right-click an item to favorite it, mark its tile with a heart, and filter favorites like any other tag.
- **Fully localized UI:** Use interface text for every language currently supported by PEAK.
- **Native multiplayer spawning:** Spawn through PEAK's built-in network RPC, with exactly one item created per click.

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

> **Compatibility notice:** Do not install ItemSpawnerEnhanced alongside [`quackandcheese-ItemSpawner`](https://thunderstore.io/c/peak/p/quackandcheese/ItemSpawner/). With both mods installed, each responds to its configured toggle key (`F5` by default), so pressing `F5` opens both item-spawner windows. Remove the original ItemSpawner before installing ItemSpawnerEnhanced.

1. **Install** `BepInExPack_PEAK`.
2. **Install ItemSpawnerEnhanced** through a mod manager. For a manual installation, extract the package and copy its `plugins/ItemSpawnerEnhanced` folder into `<PEAK>/BepInEx/plugins/`.

After installation, both plugin files should be at:

```text
<PEAK>/BepInEx/plugins/ItemSpawnerEnhanced/ItemSpawnerEnhanced.dll
<PEAK>/BepInEx/plugins/ItemSpawnerEnhanced/ItemSpawnerEnhanced.ChineseSearch.dll
```

The toggle key, tag matching mode, and catalog visibility can be changed in **`BepInEx/config/com.github.lllei.ItemSpawnerEnhanced.cfg`**. Tag matching defaults to `And`; set `TagMatchMode = Or` to show items matching any selected tag. Set `ShowAllItems = true` to include unused, test, cheat, and internal duplicate prefabs. Favorite item names are saved automatically in the same file.

See the [item tag and catalog visibility reference](https://github.com/llleixx/ItemSpawnerEnhanced/blob/main/docs/ITEM_TAGS.md) for classification rules and the complete default-hidden list.

## Multiplayer behavior

PEAK's built-in spawn RPC is used. A client with this mod can request an item for **any connected Scout available to the game**, and the room's **master client** performs the spawn. This is intentionally unrestricted and is **not a host-enforced permission system**. One click creates one item.

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
