# Search Alias Provider API

Language extensions reference `ItemSpawnerEnhanced.dll` and register an
`ISearchAliasProvider` during plugin startup. Add a hard BepInEx dependency on
`com.github.lllei.ItemSpawnerEnhanced` so the registry exists first.

```csharp
using System;
using System.Collections.Generic;
using BepInEx;
using ItemSpawnerEnhanced.Api;

[BepInPlugin("example.itemspawner.romaji", "Romaji Search", "1.0.0")]
[BepInDependency("com.github.lllei.ItemSpawnerEnhanced")]
public sealed class RomajiPlugin : BaseUnityPlugin
{
    private IDisposable? _registration;

    private void Awake() =>
        _registration = SearchAliasRegistry.Register(new RomajiProvider());

    private void OnDestroy() => _registration?.Dispose();
}

public sealed class RomajiProvider : ISearchAliasProvider
{
    public string Id => "example.japanese-romaji";

    public bool SupportsLanguage(string languageCode) => languageCode == "ja";

    public IEnumerable<string> GetAliases(SearchAliasContext context)
    {
        yield return ConvertJapaneseToRomaji(context.DisplayName);
    }

    private static string ConvertJapaneseToRomaji(string value) => value;
}
```

`SearchAliasContext` contains only strings and does not expose Unity objects:

- `ItemId`: prefab/spawn name.
- `UnlocalizedName`: the PEAK item UI source name.
- `DisplayName`: the active game-language name.
- `EnglishName`: English fallback name.
- `LanguageCode`: one of `en`, `fr`, `it`, `de`, `es-ES`, `es-419`, `pt-BR`,
  `ru`, `uk`, `zh-Hans`, `zh-Hant`, `ja`, `ko`, `pl`, or `tr`.

Provider IDs are case-insensitively unique. Registration and disposal rebuild
the search index. Exceptions from one provider are logged and isolated from
the rest of the search system.

To add a provider directly to this repository, implement the same interface
under `src/ItemSpawnerEnhanced/Core` and register it next to the built-in
Chinese provider in `Plugin.Awake`.

