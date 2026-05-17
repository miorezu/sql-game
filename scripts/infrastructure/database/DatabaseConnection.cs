using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.Sqlite;

public enum DatabaseScope
{
    Runtime,
    Content
}

public static class DatabaseConnection
{
    private static string _runtimeConnectionString;
    private static string _contentConnectionString;

    public static void Initialize(string dbPath)
    {
        InitializeRuntime(dbPath);
    }

    public static void InitializeRuntime(string dbPath)
    {
        _runtimeConnectionString = BuildConnectionString(dbPath, readOnly: false);
    }

    public static void InitializeContent(string dbPath)
    {
        _contentConnectionString = BuildConnectionString(dbPath, readOnly: true);
    }

    public static async Task<SqliteConnection> OpenConnection()
    {
        return await OpenRuntimeConnection();
    }

    public static async Task<SqliteConnection> OpenConnection(DatabaseScope scope)
    {
        return scope == DatabaseScope.Content
            ? await OpenContentConnection()
            : await OpenRuntimeConnection();
    }

    public static async Task<SqliteConnection> OpenRuntimeConnection()
    {
        return await OpenConfiguredConnection(_runtimeConnectionString, "Runtime database connection is not initialized.");
    }

    public static async Task<SqliteConnection> OpenContentConnection()
    {
        return await OpenConfiguredConnection(_contentConnectionString, "Content database connection is not initialized.");
    }

    private static async Task<SqliteConnection> OpenConfiguredConnection(string connectionString, string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new System.Exception(errorMessage);

        var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync();

        await connection.ExecuteAsync("PRAGMA foreign_keys = ON;");

        return connection;
    }

    private static string BuildConnectionString(string dbPath, bool readOnly)
    {
        var builder = new SqliteConnectionStringBuilder
        {
            DataSource = dbPath,
            Pooling = false,
            ForeignKeys = true,
            Mode = readOnly ? SqliteOpenMode.ReadOnly : SqliteOpenMode.ReadWriteCreate
        };

        return builder.ToString();
    }
}
