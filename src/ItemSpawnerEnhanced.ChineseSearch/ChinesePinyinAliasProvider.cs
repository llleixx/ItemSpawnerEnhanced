using System;
using System.Collections.Generic;
using ItemSpawnerEnhanced.Api;
using TinyPinyin;

namespace ItemSpawnerEnhanced.ChineseSearch;

internal sealed class ChinesePinyinAliasProvider : ISearchAliasProvider
{
    public string Id => "builtin.chinese-pinyin";

    public bool SupportsLanguage(string languageCode) =>
        languageCode.Equals("zh-Hans", StringComparison.OrdinalIgnoreCase) ||
        languageCode.Equals("zh-Hant", StringComparison.OrdinalIgnoreCase);

    public static void WarmUp()
    {
        const string sample = "\u9884\u70ed";
        _ = PinyinHelper.GetPinyin(sample, " ");
        _ = PinyinHelper.GetPinyin(sample, string.Empty);
        _ = PinyinHelper.GetPinyinInitials(sample, string.Empty);
    }

    public IEnumerable<string> GetAliases(SearchAliasContext context)
    {
        if (string.IsNullOrWhiteSpace(context.DisplayName))
            yield break;

        string spaced = PinyinHelper.GetPinyin(context.DisplayName, " ").ToLowerInvariant();
        string compact = PinyinHelper.GetPinyin(context.DisplayName, string.Empty).ToLowerInvariant();
        string initials = PinyinHelper.GetPinyinInitials(context.DisplayName, string.Empty).ToLowerInvariant();

        if (!string.Equals(spaced, context.DisplayName, StringComparison.OrdinalIgnoreCase))
            yield return spaced;
        if (!string.Equals(compact, spaced, StringComparison.OrdinalIgnoreCase))
            yield return compact;
        if (!string.IsNullOrWhiteSpace(initials))
            yield return initials;
    }
}
