using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using NUnit.Framework;

namespace ItemSpawnerEnhanced.Core.Tests;

public sealed class LocalizationResourceTests
{
    private static readonly string[] ExpectedLanguages =
    {
        "en", "fr", "it", "de", "es-ES", "es-419", "pt-BR", "ru", "uk",
        "zh-Hans", "zh-Hant", "ja", "ko", "pl", "tr"
    };

    [Test]
    public void EveryGameLanguageHasTheSameNonEmptyKeys()
    {
        string localizationDirectory = FindLocalizationDirectory();
        string[] files = Directory.GetFiles(localizationDirectory, "*.json");
        Assert.That(files.Select(Path.GetFileNameWithoutExtension), Is.EquivalentTo(ExpectedLanguages));

        Dictionary<string, string> english = Read(Path.Combine(localizationDirectory, "en.json"));
        foreach (string file in files)
        {
            Dictionary<string, string> values = Read(file);
            Assert.That(values.Keys, Is.EquivalentTo(english.Keys), Path.GetFileName(file));
            Assert.That(values.Values, Has.None.Empty.Or.Null, Path.GetFileName(file));
        }
    }

    private static Dictionary<string, string> Read(string path) =>
        JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(path))!;

    private static string FindLocalizationDirectory()
    {
        DirectoryInfo? directory = new(TestContext.CurrentContext.TestDirectory);
        while (directory != null)
        {
            string candidate = Path.Combine(directory.FullName, "src", "ItemSpawnerEnhanced", "Localization");
            if (Directory.Exists(candidate))
                return candidate;
            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate source localization directory.");
    }
}
