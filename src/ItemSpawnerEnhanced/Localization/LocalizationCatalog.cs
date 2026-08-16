using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;

namespace ItemSpawnerEnhanced.Localization;

internal sealed class LocalizationCatalog
{
    private readonly Dictionary<string, Dictionary<string, string>> _languages =
        new(StringComparer.OrdinalIgnoreCase);

    public LocalizationCatalog(Assembly assembly)
    {
        foreach (string resourceName in assembly.GetManifestResourceNames())
        {
            const string marker = ".Localization.";
            int markerIndex = resourceName.IndexOf(marker, StringComparison.Ordinal);
            if (markerIndex < 0 || !resourceName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                continue;

            string languageCode = resourceName.Substring(
                markerIndex + marker.Length,
                resourceName.Length - markerIndex - marker.Length - ".json".Length);

            using Stream? stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
                continue;
            using var reader = new StreamReader(stream);
            Dictionary<string, string>? values = JsonConvert.DeserializeObject<Dictionary<string, string>>(reader.ReadToEnd());
            if (values != null)
                _languages[languageCode] = values;
        }
    }

    public string Get(string languageCode, string key)
    {
        if (_languages.TryGetValue(languageCode, out Dictionary<string, string>? language) &&
            language.TryGetValue(key, out string? value) &&
            !string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        if (_languages.TryGetValue("en", out Dictionary<string, string>? english) &&
            english.TryGetValue(key, out string? fallback))
        {
            return fallback;
        }

        return key;
    }
}

