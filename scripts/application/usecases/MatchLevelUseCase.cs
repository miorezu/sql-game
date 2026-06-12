using System.Collections.Generic;
using System.Threading.Tasks;

public static class MatchLevelUseCase
{
    public static async Task<List<MatchPairData>> GetMatchPairs(LevelData levelData)
    {
        if (levelData == null)
            return new List<MatchPairData>();

        return await LevelRepository.GetMatchPairs(levelData.LevelOrder);
    }
}
