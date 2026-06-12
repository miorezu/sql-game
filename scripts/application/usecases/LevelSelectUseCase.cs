using System.Threading.Tasks;

public static class LevelSelectUseCase
{
    public static void InitializeLevelLoading()
    {
        LevelRepository.UseHybridDemoLoadingStrategy();
    }

    public static async Task<LevelButtonState> GetLevelButtonState(int levelOrder)
    {
        LevelStatus status = SaveManager.Instance.GetLevelStatus(levelOrder);

        if (levelOrder == LevelRepository.SupabaseDemoLevelOrder && status != LevelStatus.Locked)
        {
            bool isAvailable = await IsLevelAvailable(levelOrder);

            if (!isAvailable)
                status = LevelStatus.Locked;
        }

        return new LevelButtonState
        {
            LevelOrder = levelOrder,
            Status = status,
            IsDisabled = status == LevelStatus.Locked,
            BestTimeText = GetBestTimeText(levelOrder, status)
        };
    }

    public static void OpenLevel(int levelOrder)
    {
        LevelSessionUseCase.SelectLevel(levelOrder);
    }

    private static async Task<bool> IsLevelAvailable(int levelOrder)
    {
        LevelData level = await LevelRepository.GetLevelData(levelOrder);
        return level != null;
    }

    private static string GetBestTimeText(int levelOrder, LevelStatus status)
    {
        if (status != LevelStatus.Completed)
            return "";

        return SaveManager.Instance.GetBestTimeText(levelOrder);
    }
}
