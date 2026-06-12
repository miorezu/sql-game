using System.Collections.Generic;

public class TableResultComparerTests
{
    [Fact]
    public void AreTablesEqual_ReturnsTrue_WhenColumnsAndRowsMatch()
    {
        var columns = new List<string> { "id", "name" };
        var rows = new List<List<string>>
        {
            new() { "1", "Anna" },
            new() { "2", "Mark" }
        };

        bool result = TableResultComparer.AreTablesEqual(
            columns,
            rows,
            new List<string> { "id", "name" },
            new List<List<string>>
            {
                new() { "1", "Anna" },
                new() { "2", "Mark" }
            }
        );

        Assert.True(result);
    }

    [Fact]
    public void AreTablesEqual_ReturnsFalse_WhenColumnOrderDiffers()
    {
        bool result = TableResultComparer.AreTablesEqual(
            new List<string> { "id", "name" },
            new List<List<string>> { new() { "1", "Anna" } },
            new List<string> { "name", "id" },
            new List<List<string>> { new() { "Anna", "1" } }
        );

        Assert.False(result);
    }

    [Fact]
    public void AreTablesEqual_ReturnsFalse_WhenRowCountDiffers()
    {
        bool result = TableResultComparer.AreTablesEqual(
            new List<string> { "id" },
            new List<List<string>>
            {
                new() { "1" },
                new() { "2" }
            },
            new List<string> { "id" },
            new List<List<string>> { new() { "1" } }
        );

        Assert.False(result);
    }

    [Fact]
    public void AreTablesEqual_ReturnsFalse_WhenAnyCellValueDiffers()
    {
        bool result = TableResultComparer.AreTablesEqual(
            new List<string> { "id", "name" },
            new List<List<string>> { new() { "1", "Anna" } },
            new List<string> { "id", "name" },
            new List<List<string>> { new() { "1", "Olena" } }
        );

        Assert.False(result);
    }
}
