using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ItemSpawnerEnhanced.Core;
using NUnit.Framework;

namespace ItemSpawnerEnhanced.Core.Tests;

public sealed class VanillaItemVisibilityTests
{
    [TestCase("Parachute")]
    [TestCase("Warp Compass")]
    public void PlayableOrIntentionalVariant_IsVisible(string internalName)
    {
        Assert.That(VanillaItemVisibility.IsVisible(
            internalName,
            showAllItems: false,
            isFavorite: false), Is.True);
    }

    [Test]
    public void HiddenItemRespectsShowAllAndFavoriteOverrides()
    {
        const string hidden = "Clusterberry_UNUSED";
        Assert.Multiple(() =>
        {
            Assert.That(VanillaItemVisibility.IsVisible(hidden, false, false), Is.False);
            Assert.That(VanillaItemVisibility.IsVisible(hidden, true, false), Is.True);
            Assert.That(VanillaItemVisibility.IsVisible(hidden, false, true), Is.True);
        });
    }

    [Test]
    public void DocumentationListsEveryDefaultHiddenPrefab()
    {
        string document = File.ReadAllText(FindDocument());
        int hiddenSectionStart = document.IndexOf("## Default-hidden prefabs", System.StringComparison.Ordinal);
        Assert.That(hiddenSectionStart, Is.GreaterThanOrEqualTo(0));
        string hiddenSection = document.Substring(hiddenSectionStart);
        string[] documentedNames = Regex.Matches(hiddenSection, @"^\| `([^`]+)` \|", RegexOptions.Multiline)
            .Select(match => match.Groups[1].Value)
            .ToArray();

        Assert.That(
            documentedNames,
            Is.EquivalentTo(VanillaItemVisibility.DefaultHiddenInternalNames));
    }

    private static string FindDocument()
    {
        DirectoryInfo? directory = new(TestContext.CurrentContext.TestDirectory);
        while (directory != null)
        {
            string candidate = Path.Combine(directory.FullName, "docs", "ITEM_TAGS.md");
            if (File.Exists(candidate))
                return candidate;
            directory = directory.Parent;
        }

        throw new FileNotFoundException("Could not locate docs/ITEM_TAGS.md.");
    }
}
