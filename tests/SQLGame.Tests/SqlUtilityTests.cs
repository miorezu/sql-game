using System;

public class SqlUtilityTests
{
    [Theory]
    [InlineData(0, "--:--")]
    [InlineData(-5, "--:--")]
    [InlineData(5, "00:05")]
    [InlineData(65, "01:05")]
    [InlineData(3661, "01:01:01")]
    public void TimeFormatter_Format_ReturnsExpectedText(double seconds, string expected)
    {
        string result = TimeFormatter.Format(seconds);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("keyword", BlockType.Keyword)]
    [InlineData("TABLE", BlockType.Table)]
    [InlineData("column", BlockType.Column)]
    [InlineData("unknown", BlockType.Value)]
    [InlineData("", BlockType.Value)]
    public void ParseBlockType_ReturnsExpectedBlockType(string value, BlockType expected)
    {
        BlockType result = SqlBlockParser.ParseBlockType(value);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("select", KeywordTypes.SELECT)]
    [InlineData("WHERE", KeywordTypes.WHERE)]
    [InlineData("join", KeywordTypes.JOIN)]
    [InlineData("GROUP BY", KeywordTypes.GROUP_BY)]
    [InlineData("order by", KeywordTypes.ORDER_BY)]
    [InlineData("LEFT OUTER JOIN", KeywordTypes.LEFT_JOIN)]
    [InlineData("COUNT(*)", KeywordTypes.COUNT)]
    [InlineData("IS NOT", KeywordTypes.IS_NOT)]
    [InlineData("unknown", KeywordTypes.none)]
    [InlineData("", KeywordTypes.none)]
    public void ParseKeywordType_ReturnsExpectedKeywordType(string value, KeywordTypes expected)
    {
        KeywordTypes result = SqlBlockParser.ParseKeywordType(value);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetTooltip_ReturnsContextualText_ForKnownKeyword()
    {
        string tooltip = SqlKeyword.GetTooltip(BlockType.Keyword, KeywordTypes.GROUP_BY, "GROUP BY");

        Assert.Contains("Зазвичай після:", tooltip);
        Assert.Contains("Зазвичай перед:", tooltip);
        Assert.Contains("Приклад:", tooltip);
    }

    [Fact]
    public void GetTooltip_ReturnsKnownConstructionTooltip_ForStatementText()
    {
        string tooltip = SqlKeyword.GetTooltip(
            BlockType.Value,
            KeywordTypes.none,
            "INSERT INTO users (name) VALUES ('Anna');"
        );

        Assert.Contains("INSERT", tooltip);
        Assert.Contains("VALUES", tooltip);
    }

    [Fact]
    public void GetTooltip_PreservesBlockType_WhenTableNameMatchesKeyword()
    {
        string tooltip = SqlKeyword.GetTooltip(BlockType.Table, KeywordTypes.ORDER, "order");

        Assert.Contains("Таблиця бази даних.", tooltip);
        Assert.Contains("Назва: order", tooltip);
    }

    [Theory]
    [InlineData(" table ", "table")]
    [InlineData("BUILDER", "builder")]
    [InlineData(null, "")]
    public void NormalizeLevelType_ReturnsTrimmedLowercaseValue(string? value, string expected)
    {
        string result = DatabaseHelper.NormalizeLevelType(value);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void QuoteIdentifier_ReturnsQuotedIdentifier_WhenIdentifierIsSafe()
    {
        string result = DatabaseHelper.QuoteIdentifier("level_blocks");

        Assert.Equal("\"level_blocks\"", result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("level-blocks")]
    [InlineData("levels;DROP")]
    [InlineData("levels name")]
    public void EnsureSafeIdentifier_Throws_WhenIdentifierIsUnsafe(string value)
    {
        Assert.Throws<Exception>(() => DatabaseHelper.EnsureSafeIdentifier(value));
    }
}
