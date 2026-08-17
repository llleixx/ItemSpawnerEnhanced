using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace ItemSpawnerEnhanced.Core;

internal static class FavoriteItemCodec
{
    public static string Serialize(IEnumerable<string> itemNames) =>
        JsonConvert.SerializeObject(itemNames
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(value => value, StringComparer.Ordinal));

    public static HashSet<string> Deserialize(string serialized)
    {
        string[]? values = JsonConvert.DeserializeObject<string[]>(serialized);
        return new HashSet<string>(
            (values ?? Array.Empty<string>()).Where(value => !string.IsNullOrWhiteSpace(value)),
            StringComparer.Ordinal);
    }
}
