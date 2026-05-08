using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Godot;
using Microsoft.Data.Sqlite;

public partial class DatabaseManager : Node
{
    private const string TemplateDbPath = "res://database/game.db";
    private const string UserDbPath = "user://game.db";

    private static string _connectionString;

    public static DatabaseManager Instance { get; private set; }

    public override void _Ready()
    {
        Instance = this;

        // Під час розробки зручно кожного разу копіювати шаблонну БД.
        // У фінальній версії гри це можна буде замінити на InitializeConnection().
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

        _connectionString = BuildConnectionString(userPath);

        GD.Print("[DB RESET] User DB успішно скинута.");
    }

    private void InitializeConnection()
    {
        string fullPath = ProjectSettings.GlobalizePath(UserDbPath);
        _connectionString = BuildConnectionString(fullPath);
    }

    private static string BuildConnectionString(string dbPath)
    {
        return $"Data Source={dbPath};Pooling=False;Foreign Keys=True";
    }

    private static async Task<SqliteConnection> OpenConnection()
    {
        if (string.IsNullOrWhiteSpace(_connectionString))
            throw new Exception("DatabaseManager not initialized.");

        var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        // Дублюємо ввімкнення FK для надійності, бо в SQLite це налаштування працює на рівні підключення.
        await connection.ExecuteAsync("PRAGMA foreign_keys = ON;");

        return connection;
    }

    private static string NormalizeLevelType(string levelType)
    {
        return levelType?.Trim().ToLowerInvariant() ?? "";
    }

    public static async Task<QueryResult> ExecuteSql(string sql)
    {
        if (string.IsNullOrWhiteSpace(sql))
            throw new Exception("SQL is empty.");

        var result = new QueryResult();

        await using var connection = await OpenConnection();

        await using var command = connection.CreateCommand();
        command.CommandText = sql;

        string normalized = sql.TrimStart().ToUpperInvariant();

        bool returnsRows =
            normalized.StartsWith("SELECT") ||
            normalized.StartsWith("PRAGMA") ||
            normalized.StartsWith("WITH");

        if (returnsRows)
        {
            await using var reader = await command.ExecuteReaderAsync();

            result.HasRows = true;

            for (int i = 0; i < reader.FieldCount; i++)
                result.Columns.Add(reader.GetName(i));

            while (await reader.ReadAsync())
            {
                var row = new List<string>();

                for (int i = 0; i < reader.FieldCount; i++)
                {
                    object value = reader.GetValue(i);
                    row.Add(value?.ToString() ?? "NULL");
                }

                result.Rows.Add(row);
            }
        }
        else
        {
            result.HasRows = false;
            result.AffectedRows = await command.ExecuteNonQueryAsync();
        }

        return result;
    }

    public static async Task<List<List<string>>> ExecuteQuery(string sql)
    {
        var result = await ExecuteSql(sql);
        return result.Rows;
    }

    public static async Task<LevelData> GetLevelData(int levelOrder)
    {
        await using var connection = await OpenConnection();

        var level = await connection.QueryFirstOrDefaultAsync<LevelData>(
            """
            SELECT
                id                  AS Id,
                title               AS Title,
                description         AS Description,
                source_table_name   AS SourceTableName,
                expected_table_name AS ExpectedTableName,
                LOWER(level_type)   AS LevelType,
                level_order         AS LevelOrder
            FROM levels
            WHERE level_order = @LevelOrder
            LIMIT 1
            """,
            new { LevelOrder = levelOrder }
        );

        if (level == null)
            return null;

        level.LevelType = NormalizeLevelType(level.LevelType);

        var blocks = await connection.QueryAsync<string>(
            """
            SELECT block_text
            FROM level_blocks
            WHERE level_id = @LevelId
            ORDER BY block_order
            """,
            new { LevelId = level.Id }
        );

        level.SqlBlocks = blocks.ToArray();

        if (level.LevelType == "builder")
        {
            var solutionBlocks = await connection.QueryAsync<string>(
                """
                SELECT lb.block_text
                FROM builder_solution_steps bss
                JOIN level_blocks lb
                    ON lb.id = bss.expected_block_id
                   AND lb.level_id = bss.level_id
                WHERE bss.level_id = @LevelId
                ORDER BY bss.step_order
                """,
                new { LevelId = level.Id }
            );

            level.BuilderSolutionBlocks = solutionBlocks.ToArray();
        }
        else
        {
            level.BuilderSolutionBlocks = Array.Empty<string>();
        }

        GD.Print($"[DB] Level: {level.Title} (order: {level.LevelOrder})");
        GD.Print($"[DB] Type: {level.LevelType}");
        GD.Print($"[DB] Source: {level.SourceTableName}");
        GD.Print($"[DB] Expected: {level.ExpectedTableName}");
        GD.Print($"[DB] Blocks count: {level.SqlBlocks.Length}");
        GD.Print($"[DB] Builder solution count: {level.BuilderSolutionBlocks.Length}");

        return level;
    }

    public static async Task<int?> GetNextLevelOrder(int currentLevelOrder)
    {
        await using var connection = await OpenConnection();

        return await connection.QueryFirstOrDefaultAsync<int?>(
            """
            SELECT MIN(level_order)
            FROM levels
            WHERE level_order > @CurrentLevelOrder
            """,
            new { CurrentLevelOrder = currentLevelOrder }
        );
    }

    public static async Task<LevelData> GetNextLevelData(int currentLevelOrder)
    {
        var nextLevelOrder = await GetNextLevelOrder(currentLevelOrder);

        if (nextLevelOrder == null)
            return null;

        return await GetLevelData(nextLevelOrder.Value);
    }

    public static async Task<bool> HasNextLevel(int currentLevelOrder)
    {
        var nextLevelOrder = await GetNextLevelOrder(currentLevelOrder);
        return nextLevelOrder.HasValue;
    }

    public static string GetUserDbPath()
    {
        return ProjectSettings.GlobalizePath(UserDbPath);
    }

    public static async Task<bool> TableExists(string tableName)
    {
        EnsureSafeIdentifier(tableName);

        await using var connection = await OpenConnection();

        var count = await connection.ExecuteScalarAsync<int>(
            """
            SELECT COUNT(*)
            FROM sqlite_master
            WHERE type = 'table' AND name = @TableName
            """,
            new { TableName = tableName }
        );

        return count > 0;
    }

    public static async Task<List<string>> GetColumnNames(string tableName)
    {
        EnsureSafeIdentifier(tableName);

        await using var connection = await OpenConnection();

        var columns = await connection.QueryAsync<string>(
            $"SELECT name FROM pragma_table_info('{tableName.Replace("'", "''")}')"
        );

        return columns.ToList();
    }

    public static async Task<List<List<string>>> GetRows(string tableName)
    {
        EnsureSafeIdentifier(tableName);

        var queryResult = await ExecuteSql($"SELECT * FROM [{tableName}]");
        return queryResult.Rows;
    }

    public static async Task<List<List<string>>> GetOrderedRows(string tableName, List<string> columns)
    {
        EnsureSafeIdentifier(tableName);

        foreach (var column in columns)
            EnsureSafeIdentifier(column);

        string orderBy = string.Join(", ", columns.Select(c => $"[{c}]"));

        var queryResult = await ExecuteSql($"SELECT * FROM [{tableName}] ORDER BY {orderBy}");
        return queryResult.Rows;
    }

    public static async Task<List<MatchPairData>> GetMatchPairs(int levelOrder)
    {
        await using var connection = await OpenConnection();

        var pairs = await connection.QueryAsync<MatchPairData>(
            """
            SELECT
                mp.id          AS Id,
                mp.level_id    AS LevelId,
                mp.left_text   AS LeftText,
                mp.right_text  AS RightText,
                mp.pair_order  AS PairOrder
            FROM match_pairs mp
            JOIN levels l ON l.id = mp.level_id
            WHERE l.level_order = @LevelOrder
            ORDER BY mp.pair_order
            """,
            new { LevelOrder = levelOrder }
        );

        return pairs.ToList();
    }

    public static async Task<List<MatchPairData>> GetMatchPairsByLevelId(int levelId)
    {
        await using var connection = await OpenConnection();

        var pairs = await connection.QueryAsync<MatchPairData>(
            """
            SELECT
                id          AS Id,
                level_id    AS LevelId,
                left_text   AS LeftText,
                right_text  AS RightText,
                pair_order  AS PairOrder
            FROM match_pairs
            WHERE level_id = @LevelId
            ORDER BY pair_order
            """,
            new { LevelId = levelId }
        );

        return pairs.ToList();
    }

    public static async Task<List<string>> GetBuilderSolutionBlocks(int levelId)
    {
        await using var connection = await OpenConnection();

        var solutionBlocks = await connection.QueryAsync<string>(
            """
            SELECT lb.block_text
            FROM builder_solution_steps bss
            JOIN level_blocks lb
                ON lb.id = bss.expected_block_id
               AND lb.level_id = bss.level_id
            WHERE bss.level_id = @LevelId
            ORDER BY bss.step_order
            """,
            new { LevelId = levelId }
        );

        return solutionBlocks.ToList();
    }

    private static void EnsureSafeIdentifier(string value)
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