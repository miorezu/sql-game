using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Виконує довільний SQL. Використовувати лише для внутрішніх потреб (debug, SQL-рівні в грі).
/// Ніколи не передавати сюди рядки від гравця без валідації.
/// </summary>
public static class RawQueryService
{
    public static async Task<QueryResult> ExecuteSql(string sql)
    {
        if (string.IsNullOrWhiteSpace(sql))
            throw new System.Exception("SQL is empty.");

        var result = new QueryResult();

        await using var connection = await DatabaseConnection.OpenConnection();
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
}