using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.Sqlite;

public static class DatabaseConnection
{
    private static string _connectionString;

    public static void Initialize(string dbPath)
    {
        _connectionString = BuildConnectionString(dbPath);
    }

    public static async Task<SqliteConnection> OpenConnection()
    {
        if (string.IsNullOrWhiteSpace(_connectionString))
            throw new System.Exception("DatabaseConnection not initialized.");

        var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        // FK вмикається на рівні підключення в SQLite, тому потрібно щоразу
        await connection.ExecuteAsync("PRAGMA foreign_keys = ON;");

        return connection;
    }

    private static string BuildConnectionString(string dbPath)
    {
        return $"Data Source={dbPath};Pooling=False;Foreign Keys=True";
    }
}