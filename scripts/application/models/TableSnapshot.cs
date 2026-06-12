using System.Collections.Generic;

public class TableSnapshot
{
    public bool Exists { get; set; }
    public string TableName { get; set; } = "";
    public List<string> Columns { get; set; } = new();
    public List<List<string>> Rows { get; set; } = new();
}
