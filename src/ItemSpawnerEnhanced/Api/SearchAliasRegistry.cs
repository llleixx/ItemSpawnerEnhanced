using System;
using System.Collections.Generic;
using System.Linq;

namespace ItemSpawnerEnhanced.Api;

public static class SearchAliasRegistry
{
    private static readonly object Sync = new();
    private static readonly Dictionary<string, ISearchAliasProvider> Providers =
        new(StringComparer.OrdinalIgnoreCase);

    public static event Action? Changed;

    public static IDisposable Register(ISearchAliasProvider provider)
    {
        if (provider == null)
            throw new ArgumentNullException(nameof(provider));
        if (string.IsNullOrWhiteSpace(provider.Id))
            throw new ArgumentException("Alias provider ID must not be empty.", nameof(provider));

        lock (Sync)
        {
            if (Providers.ContainsKey(provider.Id))
                throw new InvalidOperationException($"A search alias provider named '{provider.Id}' is already registered.");
            Providers.Add(provider.Id, provider);
        }

        Changed?.Invoke();
        return new Registration(provider.Id);
    }

    internal static IReadOnlyList<ISearchAliasProvider> Snapshot()
    {
        lock (Sync)
            return Providers.Values.ToArray();
    }

    internal static void ClearForTests()
    {
        lock (Sync)
            Providers.Clear();
        Changed = null;
    }

    private static void Unregister(string id)
    {
        bool removed;
        lock (Sync)
            removed = Providers.Remove(id);
        if (removed)
            Changed?.Invoke();
    }

    private sealed class Registration : IDisposable
    {
        private string? _id;

        public Registration(string id) => _id = id;

        public void Dispose()
        {
            string? id = _id;
            _id = null;
            if (id != null)
                Unregister(id);
        }
    }
}

