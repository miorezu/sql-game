    using System;
using System.Collections.Generic;
using Godot;



public partial class QueryBuilder : FlowContainer
{
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
                parts.Add(block.BlockValue.Trim());
        }

        return string.Join(" ", parts)
            .Replace(" ;", ";")
            .Replace("( ", "(")
            .Replace(" )", ")")
            .Replace(" ,", ",")
            .Trim();
    }
}