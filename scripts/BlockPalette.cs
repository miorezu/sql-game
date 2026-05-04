using Godot;
using SQLGame.scripts.data;

public partial class BlockPalette : FlowContainer
{
    [Export] public bool UseMatchSide { get; private set; } = false;
    [Export] public MatchSide Side { get; private set; } = MatchSide.Left;

    public void AddMatchBlock(SqlBlock block, int pairId, string text)
    {
        if (!UseMatchSide)
            return;

        block.Init(new BlockData
        {
            Type = BlockType.Value,
            Value = text,
            KeywordType = KeywordTypes.none,
            PairId = pairId,
            MatchSide = Side
        });

        AddChild(block);
    }
    
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
        
        if (oldParent is MatchDropArea oldArea)
            oldArea.ClearBlock();

        oldParent?.RemoveChild(block);
        
        AddChild(block);
        block.IsInBuilder = false;
    }
}