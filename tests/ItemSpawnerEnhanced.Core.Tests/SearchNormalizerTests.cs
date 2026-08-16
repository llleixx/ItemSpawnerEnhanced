using ItemSpawnerEnhanced.Core;
using NUnit.Framework;

namespace ItemSpawnerEnhanced.Core.Tests;

public sealed class SearchNormalizerTests
{
    [TestCase("Crème Brûlée", true, "creme brulee")]
    [TestCase("ＡＩＲ-Horn", true, "air horn")]
    [TestCase("急救 包", false, "急救包")]
    public void Normalize_HandlesUnicodeAndSeparators(string input, bool keepSpaces, string expected)
    {
        Assert.That(SearchNormalizer.Normalize(input, keepSpaces), Is.EqualTo(expected));
    }
}

