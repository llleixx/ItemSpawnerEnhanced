using System.Linq;
using ItemSpawnerEnhanced.Api;
using ItemSpawnerEnhanced.Core;
using NUnit.Framework;

namespace ItemSpawnerEnhanced.Core.Tests;

public sealed class ChinesePinyinAliasProviderTests
{
    [Test]
    public void Provider_GeneratesSpacedCompactAndInitialAliases()
    {
        var provider = new ChinesePinyinAliasProvider();
        var context = new SearchAliasContext("FirstAidKit", "First Aid Kit", "急救包", "First Aid Kit", "zh-Hans");

        string[] aliases = provider.GetAliases(context).ToArray();

        Assert.That(aliases, Does.Contain("ji jiu bao"));
        Assert.That(aliases, Does.Contain("jijiubao"));
        Assert.That(aliases, Does.Contain("jjb"));
    }

    [TestCase("zh-Hans", true)]
    [TestCase("zh-Hant", true)]
    [TestCase("ja", false)]
    public void SupportsExpectedLanguages(string language, bool expected)
    {
        Assert.That(new ChinesePinyinAliasProvider().SupportsLanguage(language), Is.EqualTo(expected));
    }
}

