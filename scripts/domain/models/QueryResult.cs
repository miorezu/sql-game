using System.Collections.Generic;



public class QueryResult
{
    public bool HasRows { get; set; }
    public int AffectedRows { get; set; }
    public List<string> Columns { get; set; } = new();
    public List<List<string>> Rows { get; set; } = new();
}