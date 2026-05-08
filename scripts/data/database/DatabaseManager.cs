using Godot;
using Microsoft.Data.Sqlite;

public partial class DatabaseManager : Node
{
    private const string TemplateDbPath = "res://database/game.db";
    private const string UserDbPath = "user://game.db";

    public static DatabaseManager Instance { get; private set; }

    public override void _Ready()
    {
        Instance = this;

        // Під час розробки зручно кожного разу копіювати шаблонну БД.
        // У фінальній версії гри замінити на InitializeConnection().
        ResetUserDb();

        GD.Print("[DB] User DB path: " + ProjectSettings.GlobalizePath(UserDbPath));
    }

    public static void ResetUserDb()
    {
        string userPath = ProjectSettings.GlobalizePath(UserDbPath);

        GD.Print("[DB RESET] Template path: " + TemplateDbPath);
        GD.Print("[DB RESET] User path: " + userPath);

        SqliteConnection.ClearAllPools();

        if (!FileAccess.FileExists(TemplateDbPath))
        {
            GD.PrintErr("[DB RESET] Template DB не знайдена: " + TemplateDbPath);
            return;
        }

        if (FileAccess.FileExists(UserDbPath))
        {
            var removeError = DirAccess.RemoveAbsolute(userPath);

            if (removeError != Error.Ok)
            {
                GD.PrintErr("[DB RESET] Не вдалося видалити стару user DB: " + removeError);
                return;
            }

            GD.Print("[DB RESET] Стара user DB видалена.");
        }

        using var sourceFile = FileAccess.Open(TemplateDbPath, FileAccess.ModeFlags.Read);
        if (sourceFile == null)
        {
            GD.PrintErr("[DB RESET] Не вдалося відкрити template DB: " + TemplateDbPath);
            return;
        }

        var buffer = sourceFile.GetBuffer((long)sourceFile.GetLength());

        if (buffer.Length == 0)
        {
            GD.PrintErr("[DB RESET] Template DB порожня!");
            return;
        }

        using var targetFile = FileAccess.Open(UserDbPath, FileAccess.ModeFlags.Write);
        if (targetFile == null)
        {
            GD.PrintErr("[DB RESET] Не вдалося створити user DB: " + UserDbPath);
            return;
        }

        targetFile.StoreBuffer(buffer);
        targetFile.Flush();

        DatabaseConnection.Initialize(userPath);

        GD.Print("[DB RESET] User DB успішно скинута.");
    }

    private static void InitializeConnection()
    {
        string fullPath = ProjectSettings.GlobalizePath(UserDbPath);
        DatabaseConnection.Initialize(fullPath);
    }

    public static string GetUserDbPath()
    {
        return ProjectSettings.GlobalizePath(UserDbPath);
    }
}