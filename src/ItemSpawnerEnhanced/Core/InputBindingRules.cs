namespace ItemSpawnerEnhanced.Core;

internal static class InputBindingRules
{
    public static string NormalizeBindingPath(string? path)
    {
        string normalized = path?.Trim() ?? string.Empty;
        return string.Equals(normalized, "None", System.StringComparison.OrdinalIgnoreCase)
            ? string.Empty
            : normalized;
    }
}
