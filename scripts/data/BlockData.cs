namespace SQLGame.scripts.data;

public class BlockData
{
    public BlockType Type { get; set; }
    public string Value { get; set; } = string.Empty;
    public KeywordTypes KeywordType { get; set; } = KeywordTypes.none;

    public MatchSide MatchSide { get; set; } = MatchSide.None;
    public int? PairId { get; set; }
}