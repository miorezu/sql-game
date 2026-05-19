using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public sealed class RoutedLevelLoadingStrategy : ILevelLoadingStrategy
{
    private readonly ILevelLoadingStrategy _defaultStrategy;
    private readonly Dictionary<int, ILevelLoadingStrategy> _levelStrategies;

    public RoutedLevelLoadingStrategy(
        ILevelLoadingStrategy defaultStrategy,
        Dictionary<int, ILevelLoadingStrategy> levelStrategies
    )
    {
        _defaultStrategy = defaultStrategy;
        _levelStrategies = levelStrategies ?? new Dictionary<int, ILevelLoadingStrategy>();
    }

    public Task<LevelData> GetLevelData(int levelOrder)
    {
        return ResolveStrategy(levelOrder).GetLevelData(levelOrder);
    }

    public async Task<int?> GetNextLevelOrder(int currentLevelOrder)
    {
        int? defaultNextLevelOrder = await _defaultStrategy.GetNextLevelOrder(currentLevelOrder);

        int? routedNextLevelOrder = _levelStrategies.Keys
            .Where(levelOrder => levelOrder > currentLevelOrder)
            .OrderBy(levelOrder => levelOrder)
            .Cast<int?>()
            .FirstOrDefault();

        if (defaultNextLevelOrder == null)
            return routedNextLevelOrder;

        if (routedNextLevelOrder == null)
            return defaultNextLevelOrder;

        return routedNextLevelOrder.Value < defaultNextLevelOrder.Value
            ? routedNextLevelOrder
            : defaultNextLevelOrder;
    }

    public Task<List<MatchPairData>> GetMatchPairs(int levelOrder)
    {
        return ResolveStrategy(levelOrder).GetMatchPairs(levelOrder);
    }

    private ILevelLoadingStrategy ResolveStrategy(int levelOrder)
    {
        return _levelStrategies.TryGetValue(levelOrder, out var strategy)
            ? strategy
            : _defaultStrategy;
    }
}
