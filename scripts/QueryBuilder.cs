using Godot;
using System.Collections.Generic;

public partial class QueryBuilder : FlowContainer
{

    public override bool _CanDropData(Vector2 position, Variant data)
    {
        return data.VariantType == Variant.Type.Dictionary;
    }

    public override void _DropData(Vector2 position, Variant data)
    {
        var dict = data.AsGodotDictionary();
        string sqlPart = dict["value"].AsString();
        BlockType type = (BlockType)dict["type"].AsInt32();

        // Створюємо мітку
        Label lbl = new Label { Text = sqlPart };
    
        lbl.SelfModulate = SqlStyle.GetColorForType(type);

        AddChild(lbl);
    }

    public string BuildQuery()
    {
        var parts = new List<string>();
        foreach (Node child in GetChildren())
        {
            if (child is Label) //потім поміняти на склБлок
            {
                parts.Add((child as Label).Text);
            }
        }

        return string.Join(" ", parts);
    }
    
    private void OnCheckButtonPressed()
    {
        GD.Print(BuildQuery());
    }
}