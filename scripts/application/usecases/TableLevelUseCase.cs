using System.Threading.Tasks;

public static class TableLevelUseCase
{
    public static async Task PrepareLevel(LevelData levelData)
    {
        await DatabaseManager.PrepareRuntimeForTableLevel(levelData);
    }

    public static async Task<QueryResult> ExecutePlayerSql(string sql)
    {
        return await RawQueryService.ExecuteSql(sql);
    }

    public static async Task<bool> IsCompleted(LevelData levelData)
    {
        if (levelData == null)
            return false;

        return await TableLevelValidator.AreTablesEqual(
            levelData.SourceTableName,
            levelData.ExpectedTableName
        );
    }
}
