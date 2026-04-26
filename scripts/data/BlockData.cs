using Godot;
using System;

public class BlockData
{
    public BlockType Type;
    public string Value;
    public KeywordTypes KeywordType = KeywordTypes.none;

    public int PairId = -1;
    public MatchSide MatchSide = MatchSide.None;
}
