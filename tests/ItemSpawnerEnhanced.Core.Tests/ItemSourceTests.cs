using System;
using System.Collections.Generic;
using System.Linq;
using ItemSpawnerEnhanced.Core;
using NUnit.Framework;

namespace ItemSpawnerEnhanced.Core.Tests;

internal sealed class ItemSourceTests
{
    private sealed record Prefab
    {
        public string Name { get; set; } = "Item";
        public bool Alive { get; set; } = true;
    }

    private readonly List<Prefab> _objects = new();
    private readonly List<Prefab> _lookup = new();
    private readonly List<Prefab> _originals = new();

    [SetUp]
    public void Reset()
    {
        _objects.Clear();
        _lookup.Clear();
        _originals.Clear();
    }

    private ItemSource<Prefab> CreateSource(Func<IEnumerable<Prefab>>? loadOriginals = null) => new(
        () => _objects, () => _lookup, loadOriginals ?? (() => _originals),
        prefab => prefab != null && prefab.Alive, _ => { });

    [Test]
    public void MergesBothRegistriesWithoutDuplicatesOrInvalidPrefabs()
    {
        var original = new Prefab();
        var modInBoth = new Prefab { Name = "Both" };
        var lookupOnly = new Prefab { Name = "Lookup only" };
        _originals.Add(original);
        _objects.AddRange(new[] { original, modInBoth, null!, new Prefab { Alive = false } });
        _lookup.AddRange(new[] { original, modInBoth, lookupOnly });

        var snapshot = CreateSource().Capture();
        Assert.Multiple(() =>
        {
            Assert.That(snapshot.Select(item => item.Prefab), Is.EqualTo(new[] { original, modInBoth, lookupOnly }));
            Assert.That(snapshot.Select(item => item.Origin), Is.EqualTo(new[]
                { ItemOrigin.Vanilla, ItemOrigin.Modded, ItemOrigin.Modded }));
        });
    }

    [Test]
    public void OriginalEditsRemainVanillaButValueEqualClonesAreModded()
    {
        var original = new Prefab();
        _objects.Add(original);
        _originals.Add(original);
        var source = CreateSource();
        source.Capture();
        original.Name = "Changed";
        var clone = original with { };
        _lookup.Add(clone);

        var items = source.Capture();
        Assert.That(items.Select(item => item.Origin), Is.EqualTo(new[] { ItemOrigin.Vanilla, ItemOrigin.Modded }));
    }

    [Test]
    public void OnlyCatalogRebuildReadsOriginalPrefabs()
    {
        var original = new Prefab();
        _objects.Add(original);
        int reads = 0;
        var source = CreateSource(() => { reads++; return _originals; });
        var first = source.Capture();
        Assert.That(first[0].Origin, Is.EqualTo(ItemOrigin.Unknown));
        _originals.Add(original);
        Assert.That(source.IsCurrent(first), Is.True);
        Assert.That(reads, Is.EqualTo(1));
        Assert.That(source.Capture()[0].Origin, Is.EqualTo(ItemOrigin.Vanilla));
        Assert.That(reads, Is.EqualTo(2));
    }

    [Test]
    public void SnapshotDetectsLateRegistrationRemovalAndValueEqualReplacement()
    {
        var original = new Prefab();
        _originals.Add(original);
        _objects.Add(original);
        var source = CreateSource();
        var before = source.Capture();
        var mod = new Prefab { Name = "Mod" };
        _lookup.Add(mod);
        var added = source.Capture();
        Assert.That(source.IsCurrent(before), Is.False);
        _lookup[0] = mod with { };
        Assert.That(source.IsCurrent(added), Is.False);
        _lookup.Clear();
        Assert.That(source.IsCurrent(before), Is.True);
    }
}
