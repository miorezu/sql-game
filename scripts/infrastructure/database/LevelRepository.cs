using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public static class LevelRepository
{
    public const int SupabaseDemoLevelOrder = 6;

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

    public static void UseSqliteLoadingStrategy()
    {
        SetLoadingStrategy(new SqliteLevelLoadingStrategy());
    }

    public static void UseSupabaseDemoLoadingStrategy()
    {
        SetLoadingStrategy(new SupabaseLevelLoadingStrategy());
    }

    public static void UseHybridDemoLoadingStrategy()
    {
        SetLoadingStrategy(
            new RoutedLevelLoadingStrategy(
                new SqliteLevelLoadingStrategy(),
                new Dictionary<int, ILevelLoadingStrategy>
                {
                    { SupabaseDemoLevelOrder, new SupabaseLevelLoadingStrategy() },
                }
            )
        );
    }

    public static async Task<LevelData> GetLevelData(int levelOrder)
    {
        Godot.GD.Print($"[LevelRepository] Strategy: {_loadingStrategy.GetType().Name}");
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
