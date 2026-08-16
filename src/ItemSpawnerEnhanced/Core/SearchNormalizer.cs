using System.Globalization;
using System.Text;

namespace ItemSpawnerEnhanced.Core;

internal static class SearchNormalizer
{
    public static string Normalize(string? value, bool keepSpaces)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        string decomposed = value.Normalize(NormalizationForm.FormKD);
        var result = new StringBuilder(decomposed.Length);
        bool pendingSpace = false;

        foreach (char character in decomposed)
        {
            UnicodeCategory category = CharUnicodeInfo.GetUnicodeCategory(character);
            if (category is UnicodeCategory.NonSpacingMark or
                UnicodeCategory.SpacingCombiningMark or
                UnicodeCategory.EnclosingMark)
            {
                continue;
            }

            if (char.IsLetterOrDigit(character))
            {
                if (keepSpaces && pendingSpace && result.Length > 0)
                    result.Append(' ');
                result.Append(char.ToLowerInvariant(character));
                pendingSpace = false;
            }
            else if (keepSpaces)
            {
                pendingSpace = true;
            }
        }

        return result.ToString();
    }
}

