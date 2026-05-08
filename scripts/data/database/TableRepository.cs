using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;

public static class TableRepository
{
    public static async Task<bool> TableExists(string tableName)
    {
        DatabaseHelper.EnsureSafeIdentifier(tableName);

        await using var connection = await DatabaseConnection.OpenConnection();

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
        DatabaseHelper.EnsureSafeIdentifier(tableName);

        await using var connection = await DatabaseConnection.OpenConnection();

        var columns = await connection.QueryAsync<string>(
            $"SELECT name FROM pragma_table_info('{tableName}')"
        );

        return columns.ToList();
    }

    public static async Task<List<List<string>>> GetRows(string tableName, List<string> orderByColumns = null)
    {
        DatabaseHelper.EnsureSafeIdentifier(tableName);

        if (orderByColumns == null || orderByColumns.Count == 0)
        {
            var result = await RawQueryService.ExecuteSql($"SELECT * FROM [{tableName}]");
            return result.Rows;
        }

        foreach (var column in orderByColumns)
            DatabaseHelper.EnsureSafeIdentifier(column);

        string orderBy = string.Join(", ", orderByColumns.Select(c => $"[{c}]"));

        var orderedResult = await RawQueryService.ExecuteSql($"SELECT * FROM [{tableName}] ORDER BY {orderBy}");
        return orderedResult.Rows;
    }
}