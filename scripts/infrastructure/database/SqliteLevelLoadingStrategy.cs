using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Godot;

public sealed class SqliteLevelLoadingStrategy : ILevelLoadingStrategy
{
    public async Task<LevelData> GetLevelData(int levelOrder)
    {
        await using var connection = await DatabaseConnection.OpenContentConnection();

        var level = await connection.QueryFirstOrDefaultAsync<LevelData>(
            """
            SELECT
                id                    AS Id,
                title                 AS Title,
                description           AS Description,
                source_table_name     AS SourceTableName,
                expected_table_name   AS ExpectedTableName,
                LOWER(level_type)     AS LevelType,
                level_order           AS LevelOrder,
                base_xp               AS BaseXp,
                target_time_seconds   AS TargetTimeSeconds
            FROM levels
            WHERE level_order = @LevelOrder
            LIMIT 1
            """,
            new { LevelOrder = levelOrder }
        );

        if (level == null)
            return null;

        level.LevelType = DatabaseHelper.NormalizeLevelType(level.LevelType);

        var blocks = await connection.QueryAsync<LevelBlockRow>(
            """
            SELECT
                block_text AS BlockText,
                LOWER(block_type) AS BlockType
            FROM level_blocks
            WHERE level_id = @LevelId
            ORDER BY block_order
            """,
            new { LevelId = level.Id }
        );

        level.SqlBlocks = blocks
            .Select(block => new BlockData
            {
                Value = block.BlockText,
                Type = SqlBlockParser.ParseBlockType(block.BlockType),
                KeywordType = SqlBlockParser.ParseKeywordType(block.BlockText)
            })
            .ToArray();

        level.BuilderSolutionBlocks = level.LevelType == "builder" ? 
            (await BuilderRepository.GetBuilderSolutionBlocks(level.Id, connection)).ToArray() : 
            Array.Empty<string>();

        PrintLoadedLevel(level);

        return level;
    }

    public async Task<int?> GetNextLevelOrder(int currentLevelOrder)
    {
        await using var connection = await DatabaseConnection.OpenContentConnection();

        return await connection.QueryFirstOrDefaultAsync<int?>(
            """
            SELECT MIN(level_order)
            FROM levels
            WHERE level_order > @CurrentLevelOrder
            """,
            new { CurrentLevelOrder = currentLevelOrder }
        );
    }

    public async Task<List<MatchPairData>> GetMatchPairs(int levelOrder)
    {
        await using var connection = await DatabaseConnection.OpenContentConnection();

        var pairs = await connection.QueryAsync<MatchPairData>(
            """
            SELECT
                mp.id          AS Id,
                mp.left_text   AS LeftText,
                mp.right_text  AS RightText
            FROM match_pairs mp
            JOIN levels l ON l.id = mp.level_id
            WHERE l.level_order = @LevelOrder
            ORDER BY mp.pair_order
            """,
            new { LevelOrder = levelOrder }
        );

        return pairs.ToList();
    }

    private static void PrintLoadedLevel(LevelData level)
    {
        GD.Print($"[DB] Level: {level.Title} (order: {level.LevelOrder})");
        GD.Print($"[DB] Type: {level.LevelType}");
        GD.Print($"[DB] Source: {level.SourceTableName}");
        GD.Print($"[DB] Expected: {level.ExpectedTableName}");
        GD.Print($"[DB] Base XP: {level.BaseXp}");
        GD.Print($"[DB] Target time: {level.TargetTimeSeconds}");
        GD.Print($"[DB] Blocks count: {level.SqlBlocks.Length}");
        GD.Print($"[DB] Builder solution count: {level.BuilderSolutionBlocks.Length}");
    }

    private sealed class LevelBlockRow
    {
        public string BlockText { get; set; }
        public string BlockType { get; set; }
    }
}
