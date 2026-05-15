using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.Sqlite;

public static class BuilderRepository
{
    public static async Task<List<string>> GetBuilderSolutionBlocks(int levelId)
    {
        await using var connection = await DatabaseConnection.OpenConnection();
        return await GetBuilderSolutionBlocks(levelId, connection);
    }

    // Перевантаження для повторного використання існуючого з'єднання (наприклад, з LevelRepository)
    internal static async Task<List<string>> GetBuilderSolutionBlocks(int levelId, SqliteConnection connection)
    {
        var solutionBlocks = await connection.QueryAsync<string>(
            """
            SELECT lb.block_text
            FROM builder_solution_steps bss
            JOIN level_blocks lb
                ON lb.id = bss.expected_block_id
               AND lb.level_id = bss.level_id
            WHERE bss.level_id = @LevelId
            ORDER BY bss.step_order
            """,
            new { LevelId = levelId }
        );

        return solutionBlocks.ToList();
    }
}