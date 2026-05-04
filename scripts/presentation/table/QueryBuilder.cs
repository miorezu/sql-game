using System;
using System.Collections.Generic;
using Godot;



public partial class QueryBuilder : FlowContainer
{
    [Export] private TableLevel _tableLevel;

    public override bool _CanDropData(Vector2 position, Variant data)
    {
        if (data.VariantType != Variant.Type.Dictionary)
            return false;

        var dict = data.AsGodotDictionary();

        if (!dict.ContainsKey("block"))
            return false;

        var block = dict["block"].As<SqlBlock>();
        if (block == null)
            return false;

        return !block.IsInBuilder;
    }

    public override void _DropData(Vector2 position, Variant data)
    {
        var dict = data.AsGodotDictionary();
        var block = dict["block"].As<SqlBlock>();

        if (block == null)
            return;

        var oldParent = block.GetParent();
        if (oldParent != null)
            oldParent.RemoveChild(block);

        AddChild(block);
        block.IsInBuilder = true;
    }

    public string BuildQuery()
    {
        var parts = new List<string>();

        foreach (Node child in GetChildren())
        {
            if (child is SqlBlock block)
                parts.Add(block.Text);
        }

        return string.Join(" ", parts);
    }

    private async void OnCheckButtonPressed()
    {
        string sql = BuildQuery();
        GD.Print("[SQL] " + sql);

        if (_tableLevel == null)
        {
            GD.PrintErr("[QueryBuilder] TableLevel не призначено в Inspector.");
            return;
        }

        try
        {
            QueryResult result = await _tableLevel.ExecutePlayerSql(sql);

            if (!result.HasRows)
            {
                GD.Print($"[SQL] Query executed. Affected rows: {result.AffectedRows}");
                return;
            }

            if (result.Rows.Count == 0)
            {
                GD.Print("[SQL] Query executed, but returned 0 rows.");
                return;
            }

            foreach (var row in result.Rows)
            {
                string rowText = string.Join(" | ", row);
                GD.Print(rowText);
            }
        }
        catch (Exception e)
        {
            GD.PrintErr($"[SQL Error]: {e.Message}");
        }
    }
}