using Godot;

public partial class XpCalculator : Node
{
    private const int PerfectReplayBonus = 5;

    private const float MinXpMultiplier = 0.3f;
    private const float MaxXpMultiplier = 1.3f;
    private const float MaxTimeBonusMultiplier = 0.3f;
    private const float WrongPenaltyMultiplier = 0.1f;

    public static int CalculateLevelXp(
        int baseXp,
        float elapsedSeconds,
        float targetSeconds,
        int wrongAttempts
    )
    {
        int minXp = Mathf.RoundToInt(baseXp * MinXpMultiplier);
        int maxXp = CalculateMaxXp(baseXp);

        int timeBonus = CalculateTimeBonus(baseXp, elapsedSeconds, targetSeconds);
        int wrongPenalty = wrongAttempts * Mathf.RoundToInt(baseXp * WrongPenaltyMultiplier);

        int calculatedXp = baseXp + timeBonus - wrongPenalty;

        return Mathf.Clamp(calculatedXp, minXp, maxXp);
    }

    public static int CalculateRewardXp(
        int calculatedXp,
        int previousBestXp,
        int maxXp
    )
    {
        if (previousBestXp >= maxXp)
            return PerfectReplayBonus;

        if (calculatedXp > previousBestXp)
            return calculatedXp - previousBestXp;

        return 0;
    }

    public static int CalculateMaxXp(int baseXp)
    {
        return Mathf.RoundToInt(baseXp * MaxXpMultiplier);
    }

    private static int CalculateTimeBonus(
        int baseXp,
        float elapsedSeconds,
        float targetSeconds
    )
    {
        if (targetSeconds <= 0)
            return 0;

        float timeRatio = elapsedSeconds / targetSeconds;

        if (timeRatio <= 0.5f)
            return Mathf.RoundToInt(baseXp * MaxTimeBonusMultiplier);

        if (timeRatio <= 0.75f)
            return Mathf.RoundToInt(baseXp * 0.2f);

        if (timeRatio <= 1.0f)
            return Mathf.RoundToInt(baseXp * 0.1f);

        return 0;
    }
}