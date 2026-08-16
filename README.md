# ItemSpawnerEnhanced

A client-side PEAK item spawner with multilingual search, Chinese pinyin
matching, spectator-aware targeting, and support for spawning items for any
connected player.

## Features

- Press `F5` to open a responsive item browser.
- Search the current language, English name, prefab name, or unlocalized name.
- Match exact names, word prefixes, substrings, and small typing mistakes.
- Search Simplified or Traditional Chinese with full pinyin or initials.
- Use Smart Target to spawn for yourself while alive and for the currently
  spectated player after death.
- Select any initialized player in the room from the target dropdown.
- Automatically discovers items added to PEAK's item database by other mods.
- Includes localized UI text for every language currently supported by PEAK.

## Installation

1. Install `BepInExPack_PEAK`.
2. Remove the original `quackandcheese-ItemSpawner`; the two plugins are
   intentionally incompatible.
3. Install the ItemSpawnerEnhanced package through a mod manager or place its
   plugin folder under `BepInEx/plugins`.

The toggle key can be changed in
`BepInEx/config/com.github.lllei.ItemSpawnerEnhanced.cfg`.

## Multiplayer behavior

PEAK's built-in spawn RPC is used. A client with this mod can request an item
for any initialized player, and the room's master client performs the spawn.
This is intentionally unrestricted and is not a host-enforced permission
system. One click creates one item.

## Language extensions

Third-party mods can register aliases without referencing Unity types. See
[the search provider API guide](docs/SEARCH_PROVIDER_API.md).

## Building

Copy `PeakGameDir.props.example` to `PeakGameDir.props`, set the PEAK path, then
run:

```powershell
dotnet test .\tests\ItemSpawnerEnhanced.Core.Tests\ItemSpawnerEnhanced.Core.Tests.csproj -c Release
dotnet build .\src\ItemSpawnerEnhanced\ItemSpawnerEnhanced.csproj -c Release
dotnet msbuild .\src\ItemSpawnerEnhanced\ItemSpawnerEnhanced.csproj -t:Deploy -p:Configuration=Release
powershell -NoProfile -ExecutionPolicy Bypass -File .\build\Package.ps1
```

## 中文说明

ItemSpawnerEnhanced 是一个纯客户端 PEAK 物品生成器。按 `F5` 打开界面，
可以用当前游戏语言、英文、物品内部名称、中文全拼或拼音首字母搜索。

“智能目标”会在存活时选择自己，在死亡观战时选择当前被观战的玩家；
也可以从右侧下拉框手动选择房间内的其他玩家。每次点击只生成一个物品。

安装增强版前需要移除原 ItemSpawner。这个 Mod 使用游戏原生生成 RPC，
不会强制实施房主权限限制。
