using System;
using ItemSpawnerEnhanced.Core;
using Newtonsoft.Json;
using NUnit.Framework;

namespace ItemSpawnerEnhanced.Core.Tests;

public sealed class FavoriteItemCodecTests
{
    [Test]
    public void RoundTrip_DeduplicatesAndUsesStableOrdinalOrder()
    {
        string serialized = FavoriteItemCodec.Serialize(new[] { "Rope Cannon", "包", "Rope Cannon", "Backpack" });

        Assert.Multiple(() =>
        {
            Assert.That(serialized, Is.EqualTo("[\"Backpack\",\"Rope Cannon\",\"包\"]"));
            Assert.That(FavoriteItemCodec.Deserialize(serialized),
                Is.EquivalentTo(new[] { "Backpack", "Rope Cannon", "包" }));
        });
    }

    [Test]
    public void Serialize_HandlesSpecialCharactersAndSkipsBlankNames()
    {
        string serialized = FavoriteItemCodec.Serialize(new[] { "Quote\"Item", "Path\\Item", "", " " });

        Assert.That(FavoriteItemCodec.Deserialize(serialized),
            Is.EquivalentTo(new[] { "Quote\"Item", "Path\\Item" }));
    }

    [Test]
    public void Deserialize_RejectsMalformedJson()
    {
        Assert.Throws<JsonReaderException>(() => FavoriteItemCodec.Deserialize("[not-json]"));
    }
}
