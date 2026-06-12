using System.Threading.Tasks;

public static class CompleteLevelUseCase
{
    public static async Task<LevelCompleteResult> CompleteLevel(
        LevelData levelData,
        float elapsedSeconds,
        int wrongAttempts
    )
    {
        int calculatedXp = XpCalculator.CalculateLevelXp(
            levelData.BaseXp,
            wrongAttempts
        );

        int previousBestXp = GetPreviousBestXp(levelData.LevelOrder);

        int rewardXp = XpCalculator.CalculateRewardXp(
            previousBestXp,
            levelData.BaseXp,
            wrongAttempts
        );

        SaveManager.Instance.Data.Xp += rewardXp;

        if (calculatedXp > previousBestXp)
            SaveManager.Instance.Data.BestLevelXp[levelData.LevelOrder] = calculatedXp;

        SaveManager.Instance.RecordLevelComplete(levelData.LevelOrder, elapsedSeconds);

        return new LevelCompleteResult
        {
            LevelData = levelData,
            CalculatedXp = calculatedXp,
            RewardXp = rewardXp,
            ElapsedSeconds = elapsedSeconds,
            WrongAttempts = wrongAttempts,
            HasNextLevel = await LevelSessionUseCase.HasNextLevel(levelData)
        };
    }

    private static int GetPreviousBestXp(int levelOrder)
    {
        if (SaveManager.Instance.Data.BestLevelXp.ContainsKey(levelOrder))
            return SaveManager.Instance.Data.BestLevelXp[levelOrder];

        return 0;
    }
}
