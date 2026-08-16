using System.Collections.Generic;
using ItemSpawnerEnhanced.Core;
using NUnit.Framework;

namespace ItemSpawnerEnhanced.Core.Tests;

public sealed class SearchIndexTests
{
    [Test]
    public void Search_RanksDisplayNameBeforeFallbackAliases()
    {
        var index = BuildIndex();

        Assert.That(index.Search("rope"), Is.EqualTo(new[] { "rope", "rope-cannon" }));
    }

    [Test]
    public void Search_MatchesContainsAndProviderAliases()
    {
        var index = BuildIndex();

        Assert.That(index.Search("cannon"), Is.EqualTo(new[] { "rope-cannon" }));
        Assert.That(index.Search("ssq"), Is.EqualTo(new[] { "rope-cannon" }));
    }

    [Test]
    public void Search_UsesBoundedTypoTolerance()
    {
        var index = BuildIndex();

        Assert.That(index.Search("ropw"), Is.EqualTo(new[] { "rope" }));
        Assert.That(index.Search("rpe"), Is.Empty);
    }

    [Test]
    public void EmptySearch_PreservesDatabaseOrder()
    {
        var index = BuildIndex();
        Assert.That(index.Search(string.Empty), Is.EqualTo(new[] { "rope-cannon", "rope" }));
    }

    [Test]
    public void IncrementalBuilder_MatchesConstructorResults()
    {
        var builder = new SearchIndex<string>.Builder();
        foreach ((string value, IEnumerable<SearchAliasValue> aliases) in Entries())
            builder.Add(value, aliases);

        SearchIndex<string> expected = BuildIndex();
        SearchIndex<string> actual = builder.Build();

        Assert.Multiple(() =>
        {
            Assert.That(actual.Search(string.Empty), Is.EqualTo(expected.Search(string.Empty)));
            Assert.That(actual.Search("rope"), Is.EqualTo(expected.Search("rope")));
            Assert.That(actual.Search("ssq"), Is.EqualTo(expected.Search("ssq")));
            Assert.That(actual.Search("ropw"), Is.EqualTo(expected.Search("ropw")));
        });
    }

    private static SearchIndex<string> BuildIndex() => new(Entries());

    private static IEnumerable<(string, IEnumerable<SearchAliasValue>)> Entries() => new[]
    {
        (
            "rope-cannon",
            (IEnumerable<SearchAliasValue>)new[]
            {
                new SearchAliasValue("绳索枪", SearchAliasPriority.Display),
                new SearchAliasValue("Rope Cannon", SearchAliasPriority.English),
                new SearchAliasValue("sheng suo qiang", SearchAliasPriority.Provider),
                new SearchAliasValue("ssq", SearchAliasPriority.Provider)
            }),
        (
            "rope",
            (IEnumerable<SearchAliasValue>)new[]
            {
                new SearchAliasValue("Rope", SearchAliasPriority.Display),
                new SearchAliasValue("Rope", SearchAliasPriority.English)
            })
    };
}
