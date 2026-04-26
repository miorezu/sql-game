using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.Sqlite;
using SQLGame.scripts.data;


public partial class DatabaseManager : Node
{
    private const string SourceDbPath = "res://database/game.db";
    private const string TargetDbPath = "user://game.db";

    public static DatabaseManager Instance { get; private set; }

    private static string _connectionString;

    public override void _Ready()
    {
        Instance = this;

        CopyDatabaseIfNeeded();
        InitializeConnection();

        GD.Print("[DB] User DB path: " + ProjectSettings.GlobalizePath(TargetDbPath));
    }

    private void CopyDatabaseIfNeeded()
    {
        if (FileAccess.FileExists(TargetDbPath))
            DirAccess.RemoveAbsolute(ProjectSettings.GlobalizePath(TargetDbPath));

        using var sourceFile = FileAccess.Open(SourceDbPath, FileAccess.ModeFlags.Read);
        if (sourceFile == null)
        {
            GD.PrintErr("[DB] Не вдалося відкрити шаблонну БД: " + SourceDbPath);
            return;
        }

        var buffer = sourceFile.GetBuffer((long)sourceFile.GetLength());

        using var targetFile = FileAccess.Open(TargetDbPath, FileAccess.ModeFlags.Write);
        if (targetFile == null)
        {
            GD.PrintErr("[DB] Не вдалося створити user БД: " + TargetDbPath);
            return;
        }

        targetFile.StoreBuffer(buffer);
        GD.Print("[DB] БД скопійована заново в user://");
    }

    private void InitializeConnection()
    {
        string fullPath = ProjectSettings.GlobalizePath(TargetDbPath);
        _connectionString = $"Data Source={fullPath}";
    }

    public static async Task<QueryResult> ExecuteSql(string sql)
    {
        if (string.IsNullOrWhiteSpace(_connectionString))
            throw new Exception("DatabaseManager not initialized.");

        if (string.IsNullOrWhiteSpace(sql))
            throw new Exception("SQL is empty.");

        var result = new QueryResult();

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

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

    public static async Task<LevelData> GetLevelData(string levelCode)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var level = await connection.QueryFirstOrDefaultAsync<LevelData>(
            """
            SELECT
                id AS Id,
                code AS Code,
                title AS Title,
                description AS Description,
                source_table_name AS SourceTableName,
                expected_table_name AS ExpectedTableName,
                level_type AS LevelType
            FROM levels
            WHERE code = @Code
            LIMIT 1
            """,
            new { Code = levelCode }
        );

        if (level == null)
            return null;

        var blocks = await connection.QueryAsync<string>(
            """
            SELECT block_text
            FROM level_blocks
            WHERE level_id = @LevelId
            """,
            new { LevelId = level.Id }
        );

        level.SqlBlocks = blocks.ToArray();

        GD.Print($"[DB] Level: {level.Code}");
        GD.Print($"[DB] Type: {level.LevelType}");
        GD.Print($"[DB] Source: {level.SourceTableName}");
        GD.Print($"[DB] Expected: {level.ExpectedTableName}");

        return level;
    }

    public static string GetUserDbPath()
    {
        return ProjectSettings.GlobalizePath(TargetDbPath);
    }
    
    public static async Task<string> GetNextLevelCode(string currentCode)
    {
        string sql = $@"
        SELECT code
        FROM levels
        WHERE level_order > (
            SELECT level_order FROM levels WHERE code = '{currentCode}'
        )
        ORDER BY level_order
        LIMIT 1;
    ";

        var result = await ExecuteQuery(sql);

        if (result.Count == 0)
            return null;

        return result[0][0];
    }
    


    public static async Task<bool> TableExists(string tableName)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

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

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

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

        var queryResult = await ExecuteSql(
            $"SELECT * FROM [{tableName}] ORDER BY {orderBy}"
        );

        return queryResult.Rows;
    }

    private static void EnsureSafeIdentifier(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new System.Exception("Identifier is empty.");
    
        foreach (char c in value)
        {
            if (!char.IsLetterOrDigit(c) && c != '_')
                throw new System.Exception($"Unsafe identifier: {value}");
        }
    }
    
    public static async Task<List<MatchPairData>> GetMatchPairs(string levelCode)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var pairs = await connection.QueryAsync<MatchPairData>(
            """
            SELECT 
                mp.id AS Id,
                mp.left_text AS LeftText,
                mp.right_text AS RightText
            FROM match_pairs mp
            JOIN levels l ON l.id = mp.level_id
            WHERE l.code = @LevelCode
            """,
            new { LevelCode = levelCode }
        );

        return pairs.ToList();
    }
}