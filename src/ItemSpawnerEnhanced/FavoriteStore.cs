using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using BepInEx.Logging;
using ItemSpawnerEnhanced.Core;

namespace ItemSpawnerEnhanced;

internal sealed class FavoriteStore
{
    private readonly ConfigEntry<string> _entry;
    private readonly ManualLogSource _logger;
    private HashSet<string> _itemNames;

    public FavoriteStore(ConfigEntry<string> entry, ManualLogSource logger)
    {
        _entry = entry;
        _logger = logger;
        _itemNames = Deserialize(entry.Value);
    }

    public bool IsFavorite(string itemName) => _itemNames.Contains(itemName);

    public bool TryToggle(string itemName, out bool isFavorite)
    {
        var updated = new HashSet<string>(_itemNames, StringComparer.Ordinal);
        isFavorite = !updated.Remove(itemName);
        if (isFavorite)
            updated.Add(itemName);

        string serialized = FavoriteItemCodec.Serialize(updated);
        string previousSerialized = _entry.Value;
        try
        {
            _entry.Value = serialized;
        }
        catch (Exception exception)
        {
            try
            {
                _entry.Value = previousSerialized;
            }
            catch
            {
                // Keep the in-memory favorite set authoritative until persistence works again.
            }
            isFavorite = _itemNames.Contains(itemName);
            _logger.LogError($"Failed to save favorite items: {exception}");
            return false;
        }

        _itemNames = updated;
        return true;
    }

    private HashSet<string> Deserialize(string serialized)
    {
        try
        {
            return FavoriteItemCodec.Deserialize(serialized);
        }
        catch (Exception exception)
        {
            _logger.LogWarning($"Favorite item configuration is invalid and will be ignored: {exception.Message}");
            return new HashSet<string>(StringComparer.Ordinal);
        }
    }
}
