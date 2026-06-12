using System;

public static class SqlBlockParser
{
    public static BlockType ParseBlockType(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return BlockType.Value;

        if (Enum.TryParse(value, true, out BlockType result))
            return result;

        return BlockType.Value;
    }

    public static KeywordTypes ParseKeywordType(string value)
    {
        if (SqlKeyword.TryParseKeywordType(value, out KeywordTypes result))
            return result;

        return KeywordTypes.none;
    }
}
