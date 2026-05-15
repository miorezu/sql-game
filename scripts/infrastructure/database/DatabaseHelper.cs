using System;

public static class DatabaseHelper
{
    public static string NormalizeLevelType(string levelType)
    {
        return levelType?.Trim().ToLowerInvariant() ?? "";
    }

    public static void EnsureSafeIdentifier(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new Exception("Identifier is empty.");

        foreach (char c in value)
        {
            if (!char.IsLetterOrDigit(c) && c != '_')
                throw new Exception($"Unsafe identifier: {value}");
        }
    }
}