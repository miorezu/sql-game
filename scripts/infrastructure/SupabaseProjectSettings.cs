using Godot;

public static class SupabaseProjectSettings
{
    private const string UrlPath = "supabase/url";
    private const string KeyPath = "supabase/key";
    private const string LevelsTableNamePath = "supabase/levels_table_name";

    public static string Url => GetString(UrlPath);
    public static string Key => GetString(KeyPath);
    public static string LevelsTableName => GetString(LevelsTableNamePath, "demo_levels");

    private static string GetString(string path, string fallback = "")
    {
        if (!ProjectSettings.HasSetting(path))
        {
            GD.PrintErr($"[SupabaseProjectSettings] Missing ProjectSettings value: {path}");
            return fallback;
        }

        string value = ProjectSettings.GetSetting(path).AsString();

        if (string.IsNullOrWhiteSpace(value))
        {
            GD.PrintErr($"[SupabaseProjectSettings] Empty ProjectSettings value: {path}");
            return fallback;
        }

        return value;
    }
}
