using System.Collections.Generic;
using System.Threading.Tasks;

public interface ILevelLoadingStrategy
{
    Task<LevelData> GetLevelData(int levelOrder);
    Task<int?> GetNextLevelOrder(int currentLevelOrder);
    Task<List<MatchPairData>> GetMatchPairs(int levelOrder);
}
