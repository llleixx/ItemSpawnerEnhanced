# Language Search Extensions

ItemSpawnerEnhanced lets another BepInEx plugin add language-specific search terms without changing the main mod. An extension receives an item's names, returns aliases such as transliterations or initials, and leaves matching and ranking to ItemSpawnerEnhanced.

> [!TIP]
> The shortest path is simple: **reference `ItemSpawnerEnhanced.dll` -> implement one provider -> register it from your plugin**.

## How it works

```text
Localized item name
        |
        v
Your ISearchAliasProvider ----> extra aliases
        |                         e.g. romaji, pinyin, initials
        v
ItemSpawnerEnhanced ----> normalization, matching, ranking, typo tolerance
```

- **ItemSpawnerEnhanced owns search behavior.** It collects names and aliases, normalizes them, scores matches, and rebuilds the index when the language or provider set changes.
- **Your extension owns language knowledge.** It decides which language codes it supports and converts item names into useful search aliases.
- **The dependency points one way.** A language extension references `ItemSpawnerEnhanced.dll`; the main mod does not reference language extensions or their libraries.

## Choose a distribution model

| Goal | Recommended model | Result |
| --- | --- | --- |
| Publish and maintain your own language support | **Standalone companion mod** | Your package depends on ItemSpawnerEnhanced and registers its provider at runtime. |
| Add an officially maintained language to this repository | **Bundled extension** | The extension remains a separate language DLL but ships in the main package. |

Both models use the same provider API. Start with the standalone workflow below; the repository-specific steps are listed later.

## Quick start: standalone extension

### 1. Reference ItemSpawnerEnhanced

Start from a normal BepInEx plugin project targeting **`netstandard2.1`**. Keep your existing BepInEx and Unity references, then add a reference to the installed or downloaded main DLL:

```xml
<ItemGroup>
  <Reference Include="ItemSpawnerEnhanced" HintPath="path\to\ItemSpawnerEnhanced.dll" Private="false" />
</ItemGroup>
```

`Private="false"` prevents your build from copying the main mod into your extension package.

### 2. Implement a provider

Implement `ISearchAliasProvider`. The example below targets Japanese and assumes `RomajiConverter.Convert` comes from your chosen transliteration library:

```csharp
using System;
using System.Collections.Generic;
using ItemSpawnerEnhanced.Api;

internal sealed class RomajiAliasProvider : ISearchAliasProvider
{
    public string Id => "yourname.japanese-romaji";

    public bool SupportsLanguage(string languageCode) =>
        languageCode.Equals("ja", StringComparison.OrdinalIgnoreCase);

    public IEnumerable<string> GetAliases(SearchAliasContext context)
    {
        string romaji = RomajiConverter.Convert(context.DisplayName);
        if (!string.IsNullOrWhiteSpace(romaji))
            yield return romaji;
    }
}
```

> [!IMPORTANT]
> Return **aliases only**. Do not implement case folding, whitespace normalization, substring matching, scoring, or typo correction; the main mod applies those rules consistently to every provider.

### 3. Register it from your plugin

Declare a hard BepInEx dependency so ItemSpawnerEnhanced loads first, keep the returned registration alive, and dispose it when your plugin unloads:

```csharp
using System;
using BepInEx;
using ItemSpawnerEnhanced.Api;

[BepInPlugin("yourname.itemspawner.romaji", "ItemSpawnerEnhanced Romaji Search", "1.0.0")]
[BepInDependency(
    "com.github.lllei.ItemSpawnerEnhanced",
    BepInDependency.DependencyFlags.HardDependency)]
public sealed class Plugin : BaseUnityPlugin
{
    private IDisposable? _registration;

    private void Awake()
    {
        _registration = SearchAliasRegistry.Register(new RomajiAliasProvider());
    }

    private void OnDestroy()
    {
        _registration?.Dispose();
    }
}
```

### 4. Test and package

- **Test every supported language code**, at least one expected alias, and empty or unusual names.
- **Add ItemSpawnerEnhanced as a package dependency** when publishing to Thunderstore, in addition to the hard plugin dependency shown above.
- **Ship your language library** as a private dependency, or merge it into your extension if you want one language to equal one DLL.
- **Include its license notice** whenever you redistribute or merge third-party code.

Your installed extension should now load after ItemSpawnerEnhanced, register its provider, and trigger an automatic search-index rebuild.

## Provider API reference

```csharp
public interface ISearchAliasProvider
{
    string Id { get; }
    bool SupportsLanguage(string languageCode);
    IEnumerable<string> GetAliases(SearchAliasContext context);
}
```

### `SearchAliasContext`

The context contains strings only, so the provider class itself does not need PEAK or Unity types.

| Property | Use it for |
| --- | --- |
| `DisplayName` | The item name in the active game language; usually the best transliteration input. |
| `EnglishName` | The English fallback name. |
| `ItemId` | The internal prefab/spawn name, useful when a modded item has incomplete localization. |
| `UnlocalizedName` | The source name from the item's PEAK UI data. |
| `LanguageCode` | The active language code passed to `SupportsLanguage`. |

Supported codes are `en`, `fr`, `it`, `de`, `es-ES`, `es-419`, `pt-BR`, `ru`, `uk`, `zh-Hans`, `zh-Hant`, `ja`, `ko`, `pl`, and `tr`.

### Runtime rules

- **Use a stable, namespaced `Id`.** IDs are case-insensitively unique, and duplicate registration throws an exception.
- **Keep `SupportsLanguage` cheap.** It is called once per provider when the search index is rebuilt, not once per item.
- **Keep `GetAliases` focused.** It is called once for each item only when the provider supports the current language.
- **Return any number of aliases.** Empty aliases are ignored and normalized duplicates are removed by the main mod.
- **Expect automatic refreshes.** Registration, disposal, and game-language changes invalidate and rebuild the search index.
- **Expect exception isolation.** A failing provider is logged without disabling normal search or other providers.

## Reference implementation: Chinese search

The bundled [`ItemSpawnerEnhanced.ChineseSearch`](../src/ItemSpawnerEnhanced.ChineseSearch) extension is a complete working example. It supports `zh-Hans` and `zh-Hant` and turns each displayed item name into three aliases:

```text
绳索枪 -> sheng suo qiang
       -> shengsuoqiang
       -> ssq
```

| File | Responsibility |
| --- | --- |
| [`ChinesePinyinAliasProvider.cs`](../src/ItemSpawnerEnhanced.ChineseSearch/ChinesePinyinAliasProvider.cs) | Checks the two Chinese language codes and generates spaced pinyin, compact pinyin, and initials. |
| [`Plugin.cs`](../src/ItemSpawnerEnhanced.ChineseSearch/Plugin.cs) | Declares the hard dependency, registers the provider in `Awake`, and disposes it in `OnDestroy`. |
| [`ItemSpawnerEnhanced.ChineseSearch.csproj`](../src/ItemSpawnerEnhanced.ChineseSearch/ItemSpawnerEnhanced.ChineseSearch.csproj) | References the main project and keeps TinyPinyin and build-tool dependencies inside the language project. |
| [`ILRepack.targets`](../src/ItemSpawnerEnhanced.ChineseSearch/ILRepack.targets) | Merges the extension with TinyPinyin while keeping `ItemSpawnerEnhanced.dll` as an external dependency. |
| [`ChinesePinyinAliasProviderTests.cs`](../tests/ItemSpawnerEnhanced.ChineseSearch.Tests/ChinesePinyinAliasProviderTests.cs) | Verifies supported language codes and all three alias forms. |

The important boundary is that **TinyPinyin belongs to the Chinese extension**. The main `ItemSpawnerEnhanced` project neither references TinyPinyin nor contains the Chinese provider.

## Contributing a bundled extension

Use this checklist when submitting a language extension to this repository:

- [ ] Create `src/ItemSpawnerEnhanced.<Language>Search` targeting `netstandard2.1` and reference the main project.
- [ ] Keep the provider, BepInEx entry point, and language-library dependencies inside that project.
- [ ] Give the extension plugin and provider stable, unique IDs.
- [ ] Add a dedicated test project under `tests` without adding language dependencies to the core tests.
- [ ] Add both projects to `ItemSpawnerEnhanced.slnx` and the new tests to `.github/workflows/ci.yml`.
- [ ] Add third-party version, source, copyright, and license details to `THIRD_PARTY_NOTICES.md`.
- [ ] Merge private language libraries with an **explicit input list** if producing one DLL; never merge `ItemSpawnerEnhanced.dll` into the extension.
- [ ] Add the extension DLL to `build/Deploy.ps1`, `build/Package.ps1`, and the package script's expected archive entries.
- [ ] Add a row written in the supported language to the README's language-specific search table.
- [ ] Run core tests, extension tests, a Release build, deployment, and package validation.

> [!WARNING]
> Do not add a language provider, language-library package, or provider registration to the main `ItemSpawnerEnhanced` project. **Official** means maintained and bundled by this repository, not compiled into the core assembly.
