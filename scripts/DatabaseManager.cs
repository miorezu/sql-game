using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

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
        var safeCode = levelCode.Replace("'", "''");

        var rows = await ExecuteQuery(
            $"SELECT id, code, title, description, source_table_name, expected_table_name " +
            $"FROM levels WHERE code = '{safeCode}' LIMIT 1"
        );
        
        if (rows.Count == 0)
            return null;

        var row = rows[0];

        var sqlBlocksRows = await ExecuteQuery(
            $"SELECT block_text FROM level_blocks WHERE level_id = (SELECT id FROM levels WHERE code = '{safeCode}')"
        );

        var sqlBlocks = sqlBlocksRows
            .Select(r => r[0])
            .ToArray();
        
        return new LevelData
        {
            Id = int.Parse(row[0]),
            Code = row[1],
            Title = row[2],
            Description = row[3],
            SourceTableName = row[4],
            ExpectedTableName = row[5],
            SqlBlocks = sqlBlocks
        };
    }

    public static string GetUserDbPath()
    {
        return ProjectSettings.GlobalizePath(TargetDbPath);
    }
}