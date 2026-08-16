using System;
using System.Collections.Generic;
using ItemSpawnerEnhanced.Api;
using NUnit.Framework;

namespace ItemSpawnerEnhanced.Core.Tests;

public sealed class SearchAliasRegistryTests
{
    [SetUp]
    public void SetUp() => SearchAliasRegistry.ClearForTests();

    [TearDown]
    public void TearDown() => SearchAliasRegistry.ClearForTests();

    [Test]
    public void Register_AdvancesVersionAndDisposalUnregisters()
    {
        IDisposable registration = SearchAliasRegistry.Register(new StubProvider("test"));
        Assert.That(SearchAliasRegistry.Snapshot(), Has.Count.EqualTo(1));
        Assert.That(SearchAliasRegistry.Version, Is.EqualTo(1));

        registration.Dispose();
        Assert.That(SearchAliasRegistry.Snapshot(), Is.Empty);
        Assert.That(SearchAliasRegistry.Version, Is.EqualTo(2));

        registration.Dispose();
        Assert.That(SearchAliasRegistry.Version, Is.EqualTo(2));
    }

    [Test]
    public void Register_RejectsDuplicateIds()
    {
        using IDisposable registration = SearchAliasRegistry.Register(new StubProvider("test"));
        Assert.Throws<InvalidOperationException>(() => SearchAliasRegistry.Register(new StubProvider("TEST")));
    }

    [Test]
    public void Register_ReadsProviderIdOnce()
    {
        var provider = new CountingIdProvider();

        using IDisposable registration = SearchAliasRegistry.Register(provider);

        Assert.That(provider.ReadCount, Is.EqualTo(1));
    }

    private sealed class StubProvider : ISearchAliasProvider
    {
        public StubProvider(string id) => Id = id;
        public string Id { get; }
        public bool SupportsLanguage(string languageCode) => true;
        public IEnumerable<string> GetAliases(SearchAliasContext context) => Array.Empty<string>();
    }

    private sealed class CountingIdProvider : ISearchAliasProvider
    {
        public int ReadCount { get; private set; }
        public string Id
        {
            get
            {
                ReadCount++;
                return "counting-id";
            }
        }

        public bool SupportsLanguage(string languageCode) => true;
        public IEnumerable<string> GetAliases(SearchAliasContext context) => Array.Empty<string>();
    }
}
