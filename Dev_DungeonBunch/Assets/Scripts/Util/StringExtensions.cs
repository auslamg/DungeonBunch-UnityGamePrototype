using System.Text.RegularExpressions;

public static class StringExtensions
{
    public static string ToUnderscoreCase(this string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        // Remove all whitespace (spaces, tabs, newlines, etc.)
        value = Regex.Replace(value, @"\s+", "");

        // Insert underscores while preserving acronyms
        value = Regex.Replace(value, @"([a-z0-9])([A-Z])", "$1_$2");
        value = Regex.Replace(value, @"([A-Z]+)([A-Z][a-z])", "$1_$2");

        return value.ToLowerInvariant();
    }
}