using System;
using Godot;
using System.Collections.Generic;

public partial class QueryBuilder : FlowContainer
{
    public override bool _CanDropData(Vector2 position, Variant data)
    {
        if (data.VariantType != Variant.Type.Dictionary) //очікує саме словник
            return false;

        var dict = data.AsGodotDictionary();

        if (!dict.ContainsKey("block"))
            return false;

        var block = dict["block"].As<SqlBlock>();//Дістає зі словника реальний блок,
                                                 //який перетягують.
        if (block == null)
            return false;

        // У QueryBuilder можна кидати тільки блоки, яких ще немає в builder
        return !block.IsInBuilder;
    }

    //коли користувач вже відпустив блок та _CanDropData() повернув true
    public override void _DropData(Vector2 position, Variant data)
    {
        var dict = data.AsGodotDictionary();
        var block = dict["block"].As<SqlBlock>();

        if (block == null)
            return;

        //прибираємо блок з контейнера, де був
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

        try
        {
            var data = await DatabaseManager.ExecuteQuery(sql);

            if (data.Count == 0)
            {
                GD.Print("[SQL] The query is executed, but there are no rows.");
                return;
            }

            foreach (var row in data)
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