public static class MatchPairValidator
{
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
