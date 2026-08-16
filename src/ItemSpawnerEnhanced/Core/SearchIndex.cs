using System;
using System.Collections.Generic;
using System.Linq;

namespace ItemSpawnerEnhanced.Core;

internal enum SearchAliasPriority
{
    Internal = 100,
    Provider = 220,
    English = 260,
    Display = 400
}

internal readonly struct SearchAliasValue
{
    public SearchAliasValue(string value, SearchAliasPriority priority)
    {
        Value = value;
        Priority = priority;
    }

    public string Value { get; }
    public SearchAliasPriority Priority { get; }
}

internal sealed class SearchIndex<T>
{
    private readonly List<Entry> _entries;

    public SearchIndex(IEnumerable<(T Value, IEnumerable<SearchAliasValue> Aliases)> values)
    {
        _entries = values.Select((value, order) => new Entry(value.Value, order, value.Aliases)).ToList();
    }

    private SearchIndex(List<Entry> entries) => _entries = entries;

    public IReadOnlyList<T> Search(string? query)
    {
        string spacedQuery = SearchNormalizer.Normalize(query, keepSpaces: true);
        string compactQuery = SearchNormalizer.Normalize(query, keepSpaces: false);
        if (compactQuery.Length == 0)
            return _entries.Select(entry => entry.Value).ToArray();

        return _entries
            .Select(entry => (Entry: entry, Score: entry.Score(spacedQuery, compactQuery)))
            .Where(result => result.Score >= 0)
            .OrderByDescending(result => result.Score)
            .ThenBy(result => result.Entry.Order)
            .Select(result => result.Entry.Value)
            .ToArray();
    }

    private sealed class Entry
    {
        private readonly Alias[] _aliases;

        public Entry(T value, int order, IEnumerable<SearchAliasValue> aliases)
        {
            Value = value;
            Order = order;
            _aliases = aliases
                .Where(alias => !string.IsNullOrWhiteSpace(alias.Value))
                .Select(alias => new Alias(alias.Value, (int)alias.Priority))
                .GroupBy(alias => alias.Compact, StringComparer.Ordinal)
                .Select(group => group.OrderByDescending(alias => alias.Boost).First())
                .ToArray();
        }

        public T Value { get; }
        public int Order { get; }

        public int Score(string spacedQuery, string compactQuery)
        {
            int best = -1;
            foreach (Alias alias in _aliases)
                best = Math.Max(best, alias.Score(spacedQuery, compactQuery));
            return best;
        }
    }

    private sealed class Alias
    {
        public Alias(string value, int boost)
        {
            Spaced = SearchNormalizer.Normalize(value, keepSpaces: true);
            Compact = SearchNormalizer.Normalize(value, keepSpaces: false);
            Boost = boost;
        }

        public string Spaced { get; }
        public string Compact { get; }
        public int Boost { get; }

        public int Score(string spacedQuery, string compactQuery)
        {
            if (Compact.Length == 0)
                return -1;
            if (Compact == compactQuery)
                return 10_000 + Boost;
            if (Spaced.StartsWith(spacedQuery, StringComparison.Ordinal) || Compact.StartsWith(compactQuery, StringComparison.Ordinal))
                return 8_000 + Boost;
            if (Spaced.Split(' ').Any(token => token.StartsWith(spacedQuery, StringComparison.Ordinal)))
                return 7_000 + Boost;
            if (Spaced.Contains(spacedQuery, StringComparison.Ordinal) || Compact.Contains(compactQuery, StringComparison.Ordinal))
                return 5_000 + Boost;

            int maximumDistance = compactQuery.Length < 4 ? 0 : compactQuery.Length <= 7 ? 1 : 2;
            if (maximumDistance == 0)
                return -1;

            int distance = EditDistance.DamerauLevenshtein(Compact, compactQuery, maximumDistance);
            return distance <= maximumDistance ? 3_000 + Boost - distance * 300 : -1;
        }
    }

    internal sealed class Builder
    {
        private readonly List<Entry> _entries = new();

        public void Add(T value, IEnumerable<SearchAliasValue> aliases) =>
            _entries.Add(new Entry(value, _entries.Count, aliases));

        public SearchIndex<T> Build() => new(_entries.ToList());
    }
}
