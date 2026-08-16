using System;

namespace ItemSpawnerEnhanced.Api;

public sealed class SearchAliasContext
{
    public SearchAliasContext(
        string itemId,
        string unlocalizedName,
        string displayName,
        string englishName,
        string languageCode)
    {
        ItemId = itemId ?? throw new ArgumentNullException(nameof(itemId));
        UnlocalizedName = unlocalizedName ?? string.Empty;
        DisplayName = displayName ?? string.Empty;
        EnglishName = englishName ?? string.Empty;
        LanguageCode = languageCode ?? throw new ArgumentNullException(nameof(languageCode));
    }

    public string ItemId { get; }
    public string UnlocalizedName { get; }
    public string DisplayName { get; }
    public string EnglishName { get; }
    public string LanguageCode { get; }
}

