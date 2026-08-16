using System.Collections.Generic;

namespace ItemSpawnerEnhanced.Api;

public interface ISearchAliasProvider
{
    string Id { get; }

    bool SupportsLanguage(string languageCode);

    IEnumerable<string> GetAliases(SearchAliasContext context);
}

