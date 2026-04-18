using System.Collections.Generic;
using System.Threading.Tasks;

public static class LevelValidator
{
    public static async Task<bool> AreTablesEqual(string sourceTable, string expectedTable)
    {
        List<string> sourceColumns = await GetColumnNames(sourceTable);
        List<string> expectedColumns = await GetColumnNames(expectedTable);

        if (!AreStringListsEqual(sourceColumns, expectedColumns))
            return false;

        List<List<string>> sourceRows = await GetOrderedRows(sourceTable, sourceColumns);
        List<List<string>> expectedRows = await GetOrderedRows(expectedTable, expectedColumns);

        if (sourceRows.Count != expectedRows.Count)
            return false;

        for (int i = 0; i < sourceRows.Count; i++)
        {
            if (!AreStringListsEqual(sourceRows[i], expectedRows[i]))
                return false;
        }

        return true;
    }

    private static async Task<List<string>> GetColumnNames(string tableName)
    {
        var columns = new List<string>();

        var result = await DatabaseManager.ExecuteQuery(
            $"PRAGMA table_info([{tableName}])"
        );

        foreach (var row in result)
        {
            if (row.Count > 1)
                columns.Add(row[1]);
        }

        return columns;
    }

    private static async Task<List<List<string>>> GetOrderedRows(string tableName, List<string> columns)
    {
        string orderBy = string.Join(", ", columns.ConvertAll(c => $"[{c}]"));

        return await DatabaseManager.ExecuteQuery(
            $"SELECT * FROM [{tableName}] ORDER BY {orderBy}"
        );
    }

    private static bool AreStringListsEqual(List<string> a, List<string> b)
    {
        if (a.Count != b.Count)
            return false;

        for (int i = 0; i < a.Count; i++)
        {
            if (a[i] != b[i])
                return false;
        }

        return true;
    }
}