using System.Threading.Tasks;

public static class LevelSessionUseCase
{
    public static async Task<LevelData> GetSelectedLevelData()
    {
        int levelOrder = GameState.SelectedLevelOrder;
        return await LevelRepository.GetLevelData(levelOrder);
    }

    public static void SelectLevel(int levelOrder)
    {
        GameState.SelectedLevelOrder = levelOrder;
    }

    public static async Task<bool> SelectNextLevel(LevelData currentLevel)
    {
        if (currentLevel == null)
            return false;

        bool hasNext = await LevelRepository.HasNextLevel(currentLevel.LevelOrder);

        if (!hasNext)
            return false;

        SelectLevel(currentLevel.LevelOrder + 1);
        return true;
    }

    public static void SelectCurrentLevel(LevelData currentLevel)
    {
        if (currentLevel == null)
            return;

        SelectLevel(currentLevel.LevelOrder);
    }

    public static async Task<bool> HasNextLevel(LevelData currentLevel)
    {
        if (currentLevel == null)
            return false;

        return await LevelRepository.HasNextLevel(currentLevel.LevelOrder);
    }

    public static void CleanupRuntimeIfNeeded(LevelData levelData)
    {
        if (levelData?.LevelType == "table")
            DatabaseManager.DeleteRuntimeDb();
    }

    public static void CleanupRuntime()
    {
        DatabaseManager.DeleteRuntimeDb();
    }
}
