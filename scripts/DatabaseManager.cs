using Godot;
using System;
using System.Collections.Generic;
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
        GD.Print("[DB] User DB path: " + ProjectSettings.GlobalizePath("user://game.db"));
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

    public static async Task<List<List<string>>> ExecuteQuery(string sql)
    {
        if (string.IsNullOrWhiteSpace(_connectionString))
            throw new Exception("DatabaseManager not initialized.");

        var result = new List<List<string>>();

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = sql;

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var row = new List<string>();

            for (int i = 0; i < reader.FieldCount; i++)
            {
                object value = reader.GetValue(i);
                row.Add(value?.ToString() ?? "NULL");
            }

            result.Add(row);
        }

        return result;
        
    }
    
    public static async Task<LevelData> GetLevelData(string levelCode)
    {
        var rows = await ExecuteQuery(
            $"SELECT id, code, title, description, source_table_name, expected_table_name " +
            $"FROM levels WHERE code = '{levelCode.Replace("'", "''")}' LIMIT 1"
        );

        if (rows.Count == 0)
            return null;

        var row = rows[0];

        return new LevelData
        {
            Id = int.Parse(row[0]),
            Code = row[1],
            Title = row[2],
            Description = row[3],
            SourceTableName = row[4],
            ExpectedTableName = row[5]
        };
    }
}