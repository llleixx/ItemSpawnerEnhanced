using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ItemSpawnerEnhanced.Core;
using NUnit.Framework;

namespace ItemSpawnerEnhanced.Core.Tests;

public sealed class VanillaItemCategoriesTests
{
    [TestCase("Food", (int)ItemFilterTag.Food)]
    [TestCase("Consumable", (int)ItemFilterTag.Consumable)]
    [TestCase("Equipment", (int)ItemFilterTag.Equipment)]
    [TestCase("Deployable", (int)ItemFilterTag.Deployable)]
    [TestCase("Mystical", (int)ItemFilterTag.Mystical)]
    public void DocumentationListsEveryReviewedPrefab(string heading, int tagValue)
    {
        string document = File.ReadAllText(FindDocument());
        Match section = Regex.Match(
            document,
            $@"^### {Regex.Escape(heading)}\r?\n\r?\n(?<body>.*?)(?=^### |\z)",
            RegexOptions.Multiline | RegexOptions.Singleline);
        Assert.That(section.Success, Is.True, $"Missing reviewed mapping section '{heading}'.");

        string[] documentedNames = Regex.Matches(section.Groups["body"].Value, @"`([^`]+)`")
            .Select(match => match.Groups[1].Value)
            .ToArray();
        var tag = (ItemFilterTag)tagValue;
        string[] reviewedNames = VanillaItemCategories.ReviewedTags
            .Where(pair => pair.Value.HasFlag(tag))
            .Select(pair => pair.Key)
            .ToArray();

        Assert.That(documentedNames, Is.EquivalentTo(reviewedNames));
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
