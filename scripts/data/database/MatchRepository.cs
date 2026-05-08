using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;

public static class MatchRepository
{
    public static async Task<List<MatchPairData>> GetMatchPairs(int levelOrder)
    {
        await using var connection = await DatabaseConnection.OpenConnection();

        var pairs = await connection.QueryAsync<MatchPairData>(
            """
            SELECT
                mp.id          AS Id,
                mp.level_id    AS LevelId,
                mp.left_text   AS LeftText,
                mp.right_text  AS RightText,
                mp.pair_order  AS PairOrder
            FROM match_pairs mp
            JOIN levels l ON l.id = mp.level_id
            WHERE l.level_order = @LevelOrder
            ORDER BY mp.pair_order
            """,
            new { LevelOrder = levelOrder }
        );

        return pairs.ToList();
    }

    public static async Task<List<MatchPairData>> GetMatchPairsByLevelId(int levelId)
    {
        await using var connection = await DatabaseConnection.OpenConnection();

        var pairs = await connection.QueryAsync<MatchPairData>(
            """
            SELECT
                id          AS Id,
                level_id    AS LevelId,
                left_text   AS LeftText,
                right_text  AS RightText,
                pair_order  AS PairOrder
            FROM match_pairs
            WHERE level_id = @LevelId
            ORDER BY pair_order
            """,
            new { LevelId = levelId }
        );

        return pairs.ToList();
    }
}