using System;
using System.Collections.Generic;

public class LevelData
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }

    public string SourceTableName { get; set; }
    public string ExpectedTableName { get; set; }
    public int LevelOrder { get; set; }
    public string LevelType { get; set; }
    public BlockData[] SqlBlocks { get; set; } = Array.Empty<BlockData>();
    public string[] BuilderSolutionBlocks { get; set; } = Array.Empty<string>();

}