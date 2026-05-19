using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public static class LevelRepository
{
    private static ILevelLoadingStrategy _loadingStrategy = new SqliteLevelLoadingStrategy();

    public static ILevelLoadingStrategy LoadingStrategy => _loadingStrategy;

    public static void SetLoadingStrategy(ILevelLoadingStrategy loadingStrategy)
    {
        _loadingStrategy = loadingStrategy ?? throw new ArgumentNullException(nameof(loadingStrategy));
    }

    public static void ResetLoadingStrategy()
    {
        _loadingStrategy = new SqliteLevelLoadingStrategy();
    }

    public static async Task<LevelData> GetLevelData(int levelOrder)
    {
        return await _loadingStrategy.GetLevelData(levelOrder);
    }

    public static async Task<int?> GetNextLevelOrder(int currentLevelOrder)
    {
        return await _loadingStrategy.GetNextLevelOrder(currentLevelOrder);
    }

    public static async Task<LevelData> GetNextLevelData(int currentLevelOrder)
    {
        var nextLevelOrder = await GetNextLevelOrder(currentLevelOrder);

        if (nextLevelOrder == null)
            return null;

        return await GetLevelData(nextLevelOrder.Value);
    }

    public static async Task<bool> HasNextLevel(int currentLevelOrder)
    {
        var nextLevelOrder = await GetNextLevelOrder(currentLevelOrder);
        return nextLevelOrder.HasValue;
    }

    public static async Task<List<MatchPairData>> GetMatchPairs(int levelOrder)
    {
        return await _loadingStrategy.GetMatchPairs(levelOrder);
    }
}