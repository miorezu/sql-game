using System.Collections.Generic;
using System.Threading.Tasks;



public static class LevelValidator
{
    public static async Task<bool> AreTablesEqual(string sourceTable, string expectedTable)
    {
        List<string> sourceColumns = await DatabaseManager.GetColumnNames(sourceTable);
        List<string> expectedColumns = await DatabaseManager.GetColumnNames(expectedTable);

        if (!AreStringListsEqual(sourceColumns, expectedColumns))
            return false;

        List<List<string>> sourceRows =
            await DatabaseManager.GetOrderedRows(sourceTable, sourceColumns);

        List<List<string>> expectedRows =
            await DatabaseManager.GetOrderedRows(expectedTable, expectedColumns);

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

    public static bool AreMatchPairsCorrect(SqlBlock left, SqlBlock right)
    {
        if (left == null || right == null)
            return false;

        return left.PairId == right.PairId;
    }

    public static bool AreAllMatchPairsCorrect(MatchBuilder matchBuilder)
    {
        if (matchBuilder == null)
            return false;

        foreach (var child in matchBuilder.GetChildren())
        {
            if (child is not MatchPairSlot slot)
                continue;

            var left = slot.GetLeftBlock();
            var right = slot.GetRightBlock();
            if (!AreMatchPairsCorrect(left, right))
                return false;
        }
        return true;
    }
}