using System.Collections.Generic;

public static class TableResultComparer
{
    public static bool AreTablesEqual(
        List<string> sourceColumns,
        List<List<string>> sourceRows,
        List<string> expectedColumns,
        List<List<string>> expectedRows
    )
    {
        if (!AreStringListsEqual(sourceColumns, expectedColumns))
            return false;

        if (sourceRows.Count != expectedRows.Count)
            return false;

        for (int i = 0; i < sourceRows.Count; i++)
        {
            if (!AreStringListsEqual(sourceRows[i], expectedRows[i]))
                return false;
        }

        return true;
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
