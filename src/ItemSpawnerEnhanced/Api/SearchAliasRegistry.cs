using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ItemSpawnerEnhanced.Api;

public static class SearchAliasRegistry
{
    private static readonly object Sync = new();
    private static readonly Dictionary<string, ISearchAliasProvider> Providers =
        new(StringComparer.OrdinalIgnoreCase);
    private static int _version;

    internal static int Version => Volatile.Read(ref _version);

    public static IDisposable Register(ISearchAliasProvider provider)
    {
        if (provider == null)
            throw new ArgumentNullException(nameof(provider));
        string id = provider.Id;
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Alias provider ID must not be empty.", nameof(provider));

        lock (Sync)
        {
            if (Providers.ContainsKey(id))
                throw new InvalidOperationException($"A search alias provider named '{id}' is already registered.");
            Providers.Add(id, provider);
            Interlocked.Increment(ref _version);
        }

        return new Registration(id);
    }

    internal static IReadOnlyList<ISearchAliasProvider> Snapshot()
    {
        lock (Sync)
            return Providers.Values.ToArray();
    }

    internal static void ClearForTests()
    {
        lock (Sync)
        {
            Providers.Clear();
            Interlocked.Exchange(ref _version, 0);
        }
    }

    private static void Unregister(string id)
    {
        lock (Sync)
        {
            if (Providers.Remove(id))
                Interlocked.Increment(ref _version);
        }
    }

    private sealed class Registration : IDisposable
    {
        private string? _id;

        public Registration(string id) => _id = id;

        public void Dispose()
        {
            string? id = Interlocked.Exchange(ref _id, null);
            if (id != null)
                Unregister(id);
        }
    }
}
