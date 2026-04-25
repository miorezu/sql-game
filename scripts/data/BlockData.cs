using Godot;
using System;

public partial class BlockData : Node
{
    public BlockType Type;
    public string Value;
    public KeywordTypes KeywordType = KeywordTypes.none;
}
