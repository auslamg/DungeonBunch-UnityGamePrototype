using System.Text.RegularExpressions;
using UnityEditor;

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

    public static string FormatGuid(GUID guid)
    {
        string value = guid.ToString();

        if (string.IsNullOrEmpty(value))
            return string.Empty;

        System.Text.StringBuilder sb = new System.Text.StringBuilder(value.Length + value.Length / 4);

        for (int i = 0; i < value.Length; i++)
        {
            if (i > 0 && i % 4 == 0)
                sb.Append('-');

            sb.Append(value[i]);
        }

        return sb.ToString();
    }
}