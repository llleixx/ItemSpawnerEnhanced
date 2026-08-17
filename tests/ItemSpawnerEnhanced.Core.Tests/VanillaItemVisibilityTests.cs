using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ItemSpawnerEnhanced.Core;
using NUnit.Framework;

namespace ItemSpawnerEnhanced.Core.Tests;

public sealed class VanillaItemVisibilityTests
{
    [TestCase("Clusterberry_UNUSED")]
    [TestCase("GuidebookPage_13_FirstTeams")]
    [TestCase("C_Bishop_f")]
    [TestCase("C_Bishop_m Variant")]
    [TestCase("C_Bishop_m")]
    [TestCase("C_King")]
    [TestCase("C_Knight")]
    [TestCase("C_Pawn_f Variant")]
    [TestCase("C_Pawn_f")]
    [TestCase("C_Pawn_m")]
    [TestCase("C_Queen")]
    [TestCase("C_Rook_f Variant")]
    [TestCase("C_Rook_f")]
    [TestCase("C_Rook_m")]
    [TestCase("Mandrake_Hidden")]
    [TestCase("Parasol_Roots Variant")]
    [TestCase("ScoutmasterSoul")]
    [TestCase("Skull")]
    [TestCase("Warpsketball")]
    public void NonGameplayPrefab_IsHidden(string internalName)
    {
        Assert.That(VanillaItemVisibility.IsVisible(internalName, showAllItems: false), Is.False);
    }

    [TestCase("Kingberry Green")]
    [TestCase("C_King B")]
    [TestCase("C_King W")]
    [TestCase("Mushroom Normie Poison")]
    [TestCase("Berrynana Peel Yellow")]
    [TestCase("GuidebookPage_12_Crashout")]
    [TestCase("Mandrake")]
    [TestCase("MedicinalRoot")]
    [TestCase("Parachute")]
    [TestCase("Parasol")]
    [TestCase("RescueHook_Infinite")]
    [TestCase("ScoutCookies_Vanilla")]
    [TestCase("Warp Compass")]
    public void PlayableOrIntentionalVariant_IsVisible(string internalName)
    {
        Assert.That(VanillaItemVisibility.IsVisible(internalName, showAllItems: false), Is.True);
    }

    [TestCase("Clusterberry_UNUSED")]
    [TestCase("Parasol_Roots Variant")]
    [TestCase("C_King")]
    public void ShowAllItems_RevealsDefaultHiddenPrefab(string internalName)
    {
        Assert.That(VanillaItemVisibility.IsVisible(internalName, showAllItems: true), Is.True);
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
