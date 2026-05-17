using System;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Godot;
using Microsoft.Data.Sqlite;

public partial class DatabaseManager : Node
{
    private const string TemplateDbPath = "res://database/game.db";
    private const string ContentDbPrefix = "content_";
    private const string RuntimeDbPrefix = "level_runtime_";
    private const string DbExtension = ".db";

    private static string _contentDbGodotPath;
    private static string _runtimeDbGodotPath;

    public static DatabaseManager Instance { get; private set; }

    public override void _Ready()
    {
        Instance = this;

        CleanupStaleTemporaryDatabases();
        InitializeContentDatabase();

        GD.Print("[DB] Content database initialized.");
    }

    public override void _ExitTree()
    {
        DeleteRuntimeDb();
        DeleteContentDb();
    }

    public static async Task PrepareRuntimeForTableLevel(LevelData levelData)
    {
        if (levelData == null)
            throw new ArgumentNullException(nameof(levelData));

        DatabaseHelper.EnsureSafeIdentifier(levelData.SourceTableName);

        DeleteRuntimeDb();

        _runtimeDbGodotPath = $"user://{RuntimeDbPrefix}{levelData.LevelOrder}_{Guid.NewGuid():N}{DbExtension}";
        string runtimePath = ProjectSettings.GlobalizePath(_runtimeDbGodotPath);

        DatabaseConnection.InitializeRuntime(runtimePath);

        await using var runtimeConnection = await DatabaseConnection.OpenRuntimeConnection();
        await CopyTableFromContentToRuntime(levelData.SourceTableName, runtimeConnection);

        GD.Print("[DB] Runtime database prepared for table level " + levelData.LevelOrder);
    }

    public static void DeleteRuntimeDb()
    {
        if (string.IsNullOrWhiteSpace(_runtimeDbGodotPath))
            return;

        DeleteGodotFile(_runtimeDbGodotPath);
        _runtimeDbGodotPath = null;
    }

    public static string GetContentDbPath()
    {
        return string.IsNullOrWhiteSpace(_contentDbGodotPath)
            ? null
            : ProjectSettings.GlobalizePath(_contentDbGodotPath);
    }

    public static string GetRuntimeDbPath()
    {
        return string.IsNullOrWhiteSpace(_runtimeDbGodotPath)
            ? null
            : ProjectSettings.GlobalizePath(_runtimeDbGodotPath);
    }

    public static string GetUserDbPath()
    {
        return GetRuntimeDbPath();
    }

    private static void InitializeContentDatabase()
    {
        if (!FileAccess.FileExists(TemplateDbPath))
        {
            GD.PrintErr("[DB] Template DB not found: " + TemplateDbPath);
            return;
        }

        _contentDbGodotPath = $"user://{ContentDbPrefix}{Guid.NewGuid():N}{DbExtension}";

        if (!CopyGodotFile(TemplateDbPath, _contentDbGodotPath))
            return;

        DatabaseConnection.InitializeContent(ProjectSettings.GlobalizePath(_contentDbGodotPath));
    }

    private static async Task CopyTableFromContentToRuntime(string tableName, SqliteConnection runtimeConnection)
    {
        await using var contentConnection = await DatabaseConnection.OpenContentConnection();

        string createSql = await contentConnection.QueryFirstOrDefaultAsync<string>(
            """
            SELECT sql
            FROM sqlite_master
            WHERE type = 'table' AND name = @TableName
            LIMIT 1
            """,
            new { TableName = tableName }
        );

        if (string.IsNullOrWhiteSpace(createSql))
            throw new Exception($"Table '{tableName}' was not found in content database.");

        var columns = (await contentConnection.QueryAsync<string>(
            $"SELECT name FROM pragma_table_info('{tableName}')"
        )).ToList();

        await runtimeConnection.ExecuteAsync(createSql);

        if (columns.Count == 0)
            return;

        string quotedTable = DatabaseHelper.QuoteIdentifier(tableName);
        string quotedColumns = string.Join(", ", columns.Select(DatabaseHelper.QuoteIdentifier));

        await runtimeConnection.ExecuteAsync(
            "ATTACH DATABASE @ContentPath AS content_db;",
            new { ContentPath = GetContentDbPath() }
        );

        try
        {
            await runtimeConnection.ExecuteAsync(
                $"""
                INSERT INTO main.{quotedTable} ({quotedColumns})
                SELECT {quotedColumns}
                FROM content_db.{quotedTable}
                """
            );
        }
        finally
        {
            await runtimeConnection.ExecuteAsync("DETACH DATABASE content_db;");
        }
    }

    private static bool CopyGodotFile(string sourcePath, string targetPath)
    {
        using var sourceFile = FileAccess.Open(sourcePath, FileAccess.ModeFlags.Read);
        if (sourceFile == null)
        {
            GD.PrintErr("[DB] Failed to open source DB: " + sourcePath);
            return false;
        }

        var buffer = sourceFile.GetBuffer((long)sourceFile.GetLength());
        if (buffer.Length == 0)
        {
            GD.PrintErr("[DB] Source DB is empty: " + sourcePath);
            return false;
        }

        using var targetFile = FileAccess.Open(targetPath, FileAccess.ModeFlags.Write);
        if (targetFile == null)
        {
            GD.PrintErr("[DB] Failed to create DB copy: " + targetPath);
            return false;
        }

        targetFile.StoreBuffer(buffer);
        targetFile.Flush();
        return true;
    }

    private static void DeleteContentDb()
    {
        if (string.IsNullOrWhiteSpace(_contentDbGodotPath))
            return;

        DeleteGodotFile(_contentDbGodotPath);
        _contentDbGodotPath = null;
    }

    private static void CleanupStaleTemporaryDatabases()
    {
        var dir = DirAccess.Open("user://");
        if (dir == null)
            return;

        dir.ListDirBegin();

        while (true)
        {
            string fileName = dir.GetNext();
            if (string.IsNullOrEmpty(fileName))
                break;

            if (dir.CurrentIsDir())
                continue;

            bool isTemporaryDb =
                fileName.StartsWith(ContentDbPrefix, StringComparison.Ordinal) ||
                fileName.StartsWith(RuntimeDbPrefix, StringComparison.Ordinal) ||
                fileName == "game.db";

            if (isTemporaryDb && fileName.EndsWith(DbExtension, StringComparison.OrdinalIgnoreCase))
                DeleteGodotFile("user://" + fileName);
        }

        dir.ListDirEnd();
    }

    private static void DeleteGodotFile(string godotPath)
    {
        SqliteConnection.ClearAllPools();

        if (!FileAccess.FileExists(godotPath))
            return;

        string absolutePath = ProjectSettings.GlobalizePath(godotPath);
        var removeError = DirAccess.RemoveAbsolute(absolutePath);

        if (removeError != Error.Ok)
            GD.PrintErr("[DB] Failed to delete temporary DB: " + removeError);
    }
}
