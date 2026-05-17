using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;

public static class TableRepository
{
    public static async Task<bool> TableExists(string tableName, DatabaseScope scope = DatabaseScope.Runtime)
    {
        DatabaseHelper.EnsureSafeIdentifier(tableName);

        await using var connection = await DatabaseConnection.OpenConnection(scope);

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

    public static async Task<List<string>> GetColumnNames(string tableName, DatabaseScope scope = DatabaseScope.Runtime)
    {
        DatabaseHelper.EnsureSafeIdentifier(tableName);

        await using var connection = await DatabaseConnection.OpenConnection(scope);

        var columns = await connection.QueryAsync<string>(
            $"SELECT name FROM pragma_table_info('{tableName}')"
        );

        return columns.ToList();
    }

    public static async Task<List<List<string>>> GetRows(
        string tableName,
        List<string> orderByColumns = null,
        DatabaseScope scope = DatabaseScope.Runtime
    )
    {
        DatabaseHelper.EnsureSafeIdentifier(tableName);

        await using var connection = await DatabaseConnection.OpenConnection(scope);

        string tableSql = DatabaseHelper.QuoteIdentifier(tableName);
        string sql = $"SELECT * FROM {tableSql}";

        if (orderByColumns != null && orderByColumns.Count > 0)
        {
            foreach (var column in orderByColumns)
                DatabaseHelper.EnsureSafeIdentifier(column);

            string orderBy = string.Join(", ", orderByColumns.Select(DatabaseHelper.QuoteIdentifier));
            sql += $" ORDER BY {orderBy}";
        }

        await using var command = connection.CreateCommand();
        command.CommandText = sql;

        await using var reader = await command.ExecuteReaderAsync();
        var rows = new List<List<string>>();

        while (await reader.ReadAsync())
        {
            var row = new List<string>();

            for (int i = 0; i < reader.FieldCount; i++)
            {
                object value = reader.GetValue(i);
                row.Add(value?.ToString() ?? "NULL");
            }

            rows.Add(row);
        }

        return rows;
    }
}
