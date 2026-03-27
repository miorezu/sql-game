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

        GD.Print("[DB] User DB path: " + ProjectSettings.GlobalizePath(TargetDbPath));
    }

    private void CopyDatabaseIfNeeded()
    {
        if (FileAccess.FileExists(TargetDbPath))
        {
            GD.Print("[DB] user://game.db already exists.");
            return;
        }

        if (!FileAccess.FileExists(SourceDbPath))
        {
            GD.PrintErr("[DB] Database template not found:" + SourceDbPath);
            return;
        }

        using var sourceFile = FileAccess.Open(SourceDbPath, FileAccess.ModeFlags.Read);
        if (sourceFile == null)
        {
            GD.PrintErr("[DB] Failed to open template database: " + SourceDbPath);
            return;
        }

        var buffer = sourceFile.GetBuffer((long)sourceFile.GetLength());

        using var targetFile = FileAccess.Open(TargetDbPath, FileAccess.ModeFlags.Write);
        if (targetFile == null)
        {
            GD.PrintErr("[DB] Failed to create user database: " + TargetDbPath);
            return;
        }

        targetFile.StoreBuffer(buffer);
        GD.Print("[DB] DB copied from res:// to user://");
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
}