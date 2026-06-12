using System;

public static class XpCalculator
{
    private const int PerfectReplayBonus = 5;

    private const float WrongPenaltyMultiplier = 0.1f;

    public static int CalculateLevelXp(
        int baseXp,
        int wrongAttempts
    )
    {
        int maxXp = CalculateMaxXp(baseXp);

        int wrongPenalty = wrongAttempts * RoundToInt(baseXp * WrongPenaltyMultiplier);

        int calculatedXp = baseXp - wrongPenalty;

        return Math.Clamp(calculatedXp, 0, maxXp);
    }

    public static int CalculateRewardXp(
        int previousBestXp,
        int baseXp,
        int wrongAttempts
    )
    {
        int calculatedXp = CalculateLevelXp(baseXp, wrongAttempts);
        int maxXp = CalculateMaxXp(baseXp);

        if (previousBestXp >= maxXp && calculatedXp >= maxXp && wrongAttempts == 0)
            return PerfectReplayBonus;

        if (calculatedXp > previousBestXp)
            return calculatedXp - previousBestXp;

        return 0;
    }

    public static int CalculateMaxXp(int baseXp)
    {
        return baseXp;
    }

    private static int RoundToInt(float value)
    {
        return (int)MathF.Round(value, MidpointRounding.AwayFromZero);
    }
}
