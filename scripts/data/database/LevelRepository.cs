using System;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Godot;

public static class LevelRepository
{
    public static async Task<LevelData> GetLevelData(int levelOrder)
    {
        await using var connection = await DatabaseConnection.OpenConnection();

        var level = await connection.QueryFirstOrDefaultAsync<LevelData>(
            """
            SELECT
                id                  AS Id,
                title               AS Title,
                description         AS Description,
                source_table_name   AS SourceTableName,
                expected_table_name AS ExpectedTableName,
                LOWER(level_type)   AS LevelType,
                level_order         AS LevelOrder
            FROM levels
            WHERE level_order = @LevelOrder
            LIMIT 1
            """,
            new { LevelOrder = levelOrder }
        );

        if (level == null)
            return null;

        level.LevelType = DatabaseHelper.NormalizeLevelType(level.LevelType);

        var blocks = await connection.QueryAsync<string>(
            """
            SELECT block_text
            FROM level_blocks
            WHERE level_id = @LevelId
            ORDER BY block_order
            """,
            new { LevelId = level.Id }
        );

        level.SqlBlocks = blocks.ToArray();

        if (level.LevelType == "builder")
        {
            level.BuilderSolutionBlocks = (await BuilderRepository.GetBuilderSolutionBlocks(level.Id, connection)).ToArray();
        }
        else
        {
            level.BuilderSolutionBlocks = Array.Empty<string>();
        }

        GD.Print($"[DB] Level: {level.Title} (order: {level.LevelOrder})");
        GD.Print($"[DB] Type: {level.LevelType}");
        GD.Print($"[DB] Source: {level.SourceTableName}");
        GD.Print($"[DB] Expected: {level.ExpectedTableName}");
        GD.Print($"[DB] Blocks count: {level.SqlBlocks.Length}");
        GD.Print($"[DB] Builder solution count: {level.BuilderSolutionBlocks.Length}");

        return level;
    }

    public static async Task<int?> GetNextLevelOrder(int currentLevelOrder)
    {
        await using var connection = await DatabaseConnection.OpenConnection();

        return await connection.QueryFirstOrDefaultAsync<int?>(
            """
            SELECT MIN(level_order)
            FROM levels
            WHERE level_order > @CurrentLevelOrder
            """,
            new { CurrentLevelOrder = currentLevelOrder }
        );
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
}