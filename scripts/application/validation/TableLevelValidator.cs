using System.Collections.Generic;
using System.Threading.Tasks;

public static class TableLevelValidator
{
    public static async Task<bool> AreTablesEqual(string sourceTable, string expectedTable)
    {
        List<string> sourceColumns = await TableRepository.GetColumnNames(sourceTable, DatabaseScope.Runtime);
        List<string> expectedColumns = await TableRepository.GetColumnNames(expectedTable, DatabaseScope.Content);

        List<List<string>> sourceRows =
            await TableRepository.GetRows(sourceTable, sourceColumns, DatabaseScope.Runtime);

        List<List<string>> expectedRows =
            await TableRepository.GetRows(expectedTable, expectedColumns, DatabaseScope.Content);

        return TableResultComparer.AreTablesEqual(
            sourceColumns,
            sourceRows,
            expectedColumns,
            expectedRows
        );
    }
}
