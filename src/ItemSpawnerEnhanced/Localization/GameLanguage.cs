namespace ItemSpawnerEnhanced.Localization;

internal static class GameLanguage
{
    public static string CurrentCode => ToCode(LocalizedText.CURRENT_LANGUAGE);

    public static string ToCode(LocalizedText.Language language) => language switch
    {
        LocalizedText.Language.French => "fr",
        LocalizedText.Language.Italian => "it",
        LocalizedText.Language.German => "de",
        LocalizedText.Language.SpanishSpain => "es-ES",
        LocalizedText.Language.SpanishLatam => "es-419",
        LocalizedText.Language.BRPortuguese => "pt-BR",
        LocalizedText.Language.Russian => "ru",
        LocalizedText.Language.Ukrainian => "uk",
        LocalizedText.Language.SimplifiedChinese => "zh-Hans",
        LocalizedText.Language.TraditionalChinese => "zh-Hant",
        LocalizedText.Language.Japanese => "ja",
        LocalizedText.Language.Korean => "ko",
        LocalizedText.Language.Polish => "pl",
        LocalizedText.Language.Turkish => "tr",
        _ => "en"
    };
}
