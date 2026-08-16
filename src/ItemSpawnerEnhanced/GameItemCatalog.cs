using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;
using ItemSpawnerEnhanced.Api;
using ItemSpawnerEnhanced.Core;
using ItemSpawnerEnhanced.Localization;
using Zorro.Core;

namespace ItemSpawnerEnhanced;

internal sealed class GameItemCatalog
{
    private readonly ManualLogSource _logger;
    private SearchIndex<GameItemRecord> _index = new(Array.Empty<(GameItemRecord, IEnumerable<SearchAliasValue>)>());
    private Item[] _sourceItems = Array.Empty<Item>();

    public GameItemCatalog(ManualLogSource logger) => _logger = logger;

    public IReadOnlyList<GameItemRecord> Items { get; private set; } = Array.Empty<GameItemRecord>();

    public bool IsCurrent()
    {
        Item[] current = ItemDatabase.Instance.Objects.Where(item => item != null).ToArray();
        return current.SequenceEqual(_sourceItems);
    }

    public void Rebuild()
    {
        IEnumerator rebuild = RebuildIncrementally(int.MaxValue);
        while (rebuild.MoveNext())
        {
        }
    }

    public IEnumerator RebuildIncrementally(int batchSize)
    {
        if (batchSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(batchSize));

        string languageCode = GameLanguage.CurrentCode;
        IReadOnlyList<ISearchAliasProvider> providers = SearchAliasRegistry.Snapshot();
        var indexed = new List<(GameItemRecord, IEnumerable<SearchAliasValue>)>();
        Item[] sourceItems = ItemDatabase.Instance.Objects.Where(item => item != null).ToArray();

        for (int itemIndex = 0; itemIndex < sourceItems.Length; itemIndex++)
        {
            Item item = sourceItems[itemIndex];
            string rawName = item.UIData?.itemName ?? item.name;
            string displayName = SafeLocalizedName(item, rawName);
            string englishName = SafeEnglishName(rawName);
            var context = new SearchAliasContext(item.name, rawName, displayName, englishName, languageCode);
            var aliases = new List<SearchAliasValue>
            {
                new(displayName, SearchAliasPriority.Display),
                new(englishName, SearchAliasPriority.English),
                new(item.name, SearchAliasPriority.Internal),
                new(rawName, SearchAliasPriority.Internal)
            };

            foreach (ISearchAliasProvider provider in providers)
            {
                try
                {
                    if (!provider.SupportsLanguage(languageCode))
                        continue;
                    aliases.AddRange(provider.GetAliases(context)
                        .Where(alias => !string.IsNullOrWhiteSpace(alias))
                        .Select(alias => new SearchAliasValue(alias, SearchAliasPriority.Provider)));
                }
                catch (Exception exception)
                {
                    _logger.LogError($"Search alias provider '{provider.Id}' failed for '{item.name}': {exception}");
                }
            }

            indexed.Add((new GameItemRecord(item, displayName), aliases));
            if ((itemIndex + 1) % batchSize == 0 && itemIndex + 1 < sourceItems.Length)
                yield return null;
        }

        Items = indexed.Select(entry => entry.Item1).ToArray();
        _index = new SearchIndex<GameItemRecord>(indexed);
        _sourceItems = sourceItems;
    }

    public IReadOnlyList<GameItemRecord> Search(string query) => _index.Search(query);

    private static string SafeLocalizedName(Item item, string fallback)
    {
        try
        {
            string value = item.GetName();
            return IsUsable(value) ? value : fallback;
        }
        catch
        {
            return fallback;
        }
    }

    private static string SafeEnglishName(string rawName)
    {
        try
        {
            string key = LocalizedText.GetNameIndex(rawName);
            string value = LocalizedText.GetText(key, LocalizedText.Language.English);
            return IsUsable(value) ? value : rawName;
        }
        catch
        {
            return rawName;
        }
    }

    private static bool IsUsable(string? value) =>
        !string.IsNullOrWhiteSpace(value) && !value.StartsWith("LOC:", StringComparison.OrdinalIgnoreCase);
}

internal sealed class GameItemRecord
{
    public GameItemRecord(Item item, string displayName)
    {
        Item = item;
        DisplayName = displayName;
    }

    public Item Item { get; }
    public string DisplayName { get; }
}
