using System.Collections.Generic;
using System.Threading.Tasks;

public static class TablePreviewUseCase
{
    public static async Task<TableSnapshot> LoadTable(string tableName, TableDataSource source)
    {
        DatabaseScope scope = ToDatabaseScope(source);
        bool exists = await TableRepository.TableExists(tableName, scope);

        if (!exists)
        {
            return new TableSnapshot
            {
                Exists = false,
                TableName = tableName
            };
        }

        List<string> columns = await TableRepository.GetColumnNames(tableName, scope);
        List<List<string>> rows = await TableRepository.GetRows(tableName, null, scope);

        return new TableSnapshot
        {
            Exists = true,
            TableName = tableName,
            Columns = columns,
            Rows = rows
        };
    }

    private static DatabaseScope ToDatabaseScope(TableDataSource source)
    {
        return source == TableDataSource.Content
            ? DatabaseScope.Content
            : DatabaseScope.Runtime;
    }
}
