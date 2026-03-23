using Godot;

public partial class BlockPalette : FlowContainer
{
    public override bool _CanDropData(Vector2 position, Variant data)
    {
        if (data.VariantType != Variant.Type.Dictionary)
            return false;

        var dict = data.AsGodotDictionary();

        if (!dict.ContainsKey("block") || !dict.ContainsKey("home_container"))
            return false;

        var block = dict["block"].As<SqlBlock>();
        var homeContainer = dict["home_container"].As<FlowContainer>();

        if (block == null || homeContainer == null)
            return false;

        // Приймаємо блок назад тільки в його рідний контейнер
        // і тільки якщо він зараз уже в builder
        return block.IsInBuilder && homeContainer == this;
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
        block.IsInBuilder = false;
    }
}