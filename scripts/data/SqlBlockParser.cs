using System;
using Godot;

public static class SqlBlockParser
{
    public static BlockType ParseBlockType(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return BlockType.Value;

        if (Enum.TryParse(value, true, out BlockType result))
            return result;

        GD.PrintErr($"[SqlBlockParser] Невідомий block_type: {value}");
        return BlockType.Value;
    }

    public static KeywordTypes ParseKeywordType(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return KeywordTypes.none;

        if (Enum.TryParse(value, true, out KeywordTypes result))
            return result;

        return KeywordTypes.none;
    }
}