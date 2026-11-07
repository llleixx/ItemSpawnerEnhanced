using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace ItemSpawnerEnhanced.Core;

internal enum ItemOrigin
{
    Unknown,
    Vanilla,
    Modded
}

internal readonly struct DiscoveredItem<T> where T : class
{
    public DiscoveredItem(T prefab, ItemOrigin origin)
    {
        Prefab = prefab;
        Origin = origin;
    }

    public T Prefab { get; }
    public ItemOrigin Origin { get; }
}

internal sealed class ItemSource<T> where T : class
{
    private readonly Func<IEnumerable<T>> _loadObjects;
    private readonly Func<IEnumerable<T>> _loadLookup;
    private readonly Func<IEnumerable<T>> _loadOriginals;
    private readonly Func<T, bool> _isAlive;
    private readonly Action<Exception?> _reportUnavailableOrigins;

    public ItemSource(
        Func<IEnumerable<T>> loadObjects,
        Func<IEnumerable<T>> loadLookup,
        Func<IEnumerable<T>> loadOriginals,
        Func<T, bool> isAlive,
        Action<Exception?> reportUnavailableOrigins)
    {
        _loadObjects = loadObjects;
        _loadLookup = loadLookup;
        _loadOriginals = loadOriginals;
        _isAlive = isAlive;
        _reportUnavailableOrigins = reportUnavailableOrigins;
    }

    public bool IsCurrent(IReadOnlyList<DiscoveredItem<T>> snapshot) =>
        ReadRegisteredItems().SequenceEqual(snapshot.Select(item => item.Prefab), ReferenceComparer.Instance);

    public IReadOnlyList<DiscoveredItem<T>> Capture()
    {
        HashSet<T>? originals = LoadOriginals();
        return ReadRegisteredItems()
            .Select(prefab => new DiscoveredItem<T>(prefab, originals == null
                ? ItemOrigin.Unknown
                : originals.Contains(prefab) ? ItemOrigin.Vanilla : ItemOrigin.Modded))
            .ToArray();
    }

    private IEnumerable<T> ReadRegisteredItems() => _loadObjects().Concat(_loadLookup())
        .Where(_isAlive)
        .Distinct(ReferenceComparer.Instance);

    private HashSet<T>? LoadOriginals()
    {
        try
        {
            var originals = new HashSet<T>(_loadOriginals().Where(_isAlive), ReferenceComparer.Instance);
            if (originals.Count > 0)
                return originals;
        }
        catch (Exception exception)
        {
            _reportUnavailableOrigins(exception);
            return null;
        }
        _reportUnavailableOrigins(null);
        return null;
    }

    private sealed class ReferenceComparer : IEqualityComparer<T>
    {
        public static readonly ReferenceComparer Instance = new();
        public bool Equals(T? x, T? y) => ReferenceEquals(x, y);
        public int GetHashCode(T obj) => RuntimeHelpers.GetHashCode(obj);
    }
}
