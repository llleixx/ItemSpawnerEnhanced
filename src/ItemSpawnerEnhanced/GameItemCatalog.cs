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
    private readonly Func<bool> _showAllItems;
    private SearchIndex<GameItemRecord> _index = new(Array.Empty<(GameItemRecord, IEnumerable<SearchAliasValue>)>());
    private Item[] _sourceItems = Array.Empty<Item>();
    private bool _sourceShowAllItems;

    public GameItemCatalog(ManualLogSource logger, Func<bool> showAllItems)
    {
        _logger = logger;
        _showAllItems = showAllItems;
    }

    public IReadOnlyList<GameItemRecord> Items { get; private set; } = Array.Empty<GameItemRecord>();

    public bool IsCurrent()
    {
        Item[] current = ItemDatabase.Instance.Objects.Where(item => item != null).ToArray();
        return current.SequenceEqual(_sourceItems) && _showAllItems() == _sourceShowAllItems;
    }

    public void RebuildItems()
    {
        Item[] sourceItems = ItemDatabase.Instance.Objects.Where(item => item != null).ToArray();
        bool showAllItems = _showAllItems();
        Items = sourceItems
            .Where(item => VanillaItemVisibility.IsVisible(item.name, showAllItems))
            .Select(item =>
            {
                string rawName = item.UIData?.itemName ?? item.name;
                return new GameItemRecord(
                    item,
                    SafeLocalizedName(item, rawName),
                    ItemCategoryResolver.Resolve(item, rawName));
            }).ToArray();
        _sourceItems = sourceItems;
        _sourceShowAllItems = showAllItems;
        _index = new SearchIndex<GameItemRecord>(Array.Empty<(GameItemRecord, IEnumerable<SearchAliasValue>)>());
    }

    public IEnumerator RebuildSearchIndexIncrementally(int batchSize)
    {
        if (batchSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(batchSize));

        string languageCode = GameLanguage.CurrentCode;
        IReadOnlyList<ISearchAliasProvider> providers = SearchAliasRegistry.Snapshot();
        var activeProviders = new List<ISearchAliasProvider>(providers.Count);
        foreach (ISearchAliasProvider provider in providers)
        {
            try
            {
                if (provider.SupportsLanguage(languageCode))
                    activeProviders.Add(provider);
            }
            catch (Exception exception)
            {
                _logger.LogError($"Search alias provider '{provider.Id}' failed while checking language '{languageCode}': {exception}");
            }
        }

        var indexBuilder = new SearchIndex<GameItemRecord>.Builder();

        for (int itemIndex = 0; itemIndex < Items.Count; itemIndex++)
        {
            GameItemRecord record = Items[itemIndex];
            Item item = record.Item;
            string rawName = item.UIData?.itemName ?? item.name;
            string englishName = SafeEnglishName(rawName);
            var context = new SearchAliasContext(item.name, rawName, record.DisplayName, englishName, languageCode);
            var aliases = new List<SearchAliasValue>
            {
                new(record.DisplayName, SearchAliasPriority.Display),
                new(englishName, SearchAliasPriority.English),
                new(item.name, SearchAliasPriority.Internal),
                new(rawName, SearchAliasPriority.Internal)
            };

            foreach (ISearchAliasProvider provider in activeProviders)
            {
                try
                {
                    aliases.AddRange(provider.GetAliases(context)
                        .Where(alias => !string.IsNullOrWhiteSpace(alias))
                        .Select(alias => new SearchAliasValue(alias, SearchAliasPriority.Provider)));
                }
                catch (Exception exception)
                {
                    _logger.LogError($"Search alias provider '{provider.Id}' failed for '{item.name}': {exception}");
                }
            }

            indexBuilder.Add(record, aliases);
            if ((itemIndex + 1) % batchSize == 0 && itemIndex + 1 < Items.Count)
                yield return null;
        }

        _index = indexBuilder.Build();
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
    public GameItemRecord(Item item, string displayName, ItemFilterTag tags)
    {
        Item = item;
        DisplayName = displayName;
        Tags = tags;
    }

    public Item Item { get; }
    public string DisplayName { get; }
    public ItemFilterTag Tags { get; }
}
