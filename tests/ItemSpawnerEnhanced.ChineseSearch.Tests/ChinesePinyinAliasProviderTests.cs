using System.Linq;
using ItemSpawnerEnhanced.Api;
using NUnit.Framework;

namespace ItemSpawnerEnhanced.ChineseSearch.Tests;

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

    [Test]
    public void Provider_TransliteratesTraditionalChinese()
    {
        var provider = new ChinesePinyinAliasProvider();
        var context = new SearchAliasContext("Bandage", "Bandage", "繃帶", "Bandage", "zh-Hant");

        string[] aliases = provider.GetAliases(context).ToArray();

        Assert.That(aliases, Does.Contain("beng dai"));
        Assert.That(aliases, Does.Contain("bengdai"));
        Assert.That(aliases, Does.Contain("bd"));
    }

    [TestCase("zh-Hans", true)]
    [TestCase("zh-Hant", true)]
    [TestCase("ja", false)]
    public void SupportsExpectedLanguages(string language, bool expected)
    {
        Assert.That(new ChinesePinyinAliasProvider().SupportsLanguage(language), Is.EqualTo(expected));
    }
}
