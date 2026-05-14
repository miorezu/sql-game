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

        return block != null;
    }

    public override void _DropData(Vector2 position, Variant data)
    {
        var dict = data.AsGodotDictionary();
        var block = dict["block"].As<SqlBlock>();

        if (block == null)
            return;

        Vector2 globalPosition = position + GlobalPosition;

        int insertIndex = GetInsertIndex(globalPosition, block);

        var oldParent = block.GetParent();
        if (oldParent != null)
            oldParent.RemoveChild(block);

        AddChild(block);
        MoveChild(block, insertIndex);

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

    private int GetInsertIndex(Vector2 position, SqlBlock draggedBlock)
    {
        int index = 0;
        float closestDist = float.MaxValue;
        int closestIndex = 0;
        bool insertBefore = true;

        foreach (Node child in GetChildren())
        {
            if (child == draggedBlock)
                continue;

            if (child is not Control control)
                continue;

            Vector2 childCenter = control.GlobalPosition + control.Size / 2f;

            float dist = position.DistanceTo(childCenter);

            if (dist < closestDist)
            {
                closestDist = dist;
                closestIndex = index;
                insertBefore = position.X < childCenter.X;
            }

            index++;
        }

        return insertBefore ? closestIndex : closestIndex + 1;
    }
}