using Godot;

public partial class MatchDropArea : PanelContainer
{
    [Export] private MatchSide _acceptedSide = MatchSide.Left;

    private SqlBlock _currentBlock;
    private MatchPairSlot _pairSlot;

    public override void _Ready()
    {
        _pairSlot = GetParent().GetParent<MatchPairSlot>();
        //CustomMinimumSize = new Vector2(180, 55);
    }

    public override bool _CanDropData(Vector2 position, Variant data)
    {
        if (_currentBlock != null)
            return false;

        if (data.VariantType != Variant.Type.Dictionary)
            return false;

        var dict = data.AsGodotDictionary();

        if (!dict.ContainsKey("block"))
            return false;

        var block = dict["block"].As<SqlBlock>();

        if (block == null)
            return false;

        return block.MatchSide == _acceptedSide;
    }

    public override void _DropData(Vector2 position, Variant data)
    {
        var dict = data.AsGodotDictionary();
        var block = dict["block"].As<SqlBlock>();

        if (block == null)
            return;
        
        if (block.GetParent() is MatchDropArea oldArea)
            oldArea.ClearBlock();

        block.GetParent()?.RemoveChild(block);

        AddChild(block);
        block.IsInBuilder = true;

        _currentBlock = block;

        //_pairSlot?.CheckPair();
    }
    
    public void ClearBlock()
    {
        _currentBlock = null;
        _pairSlot?.ResetVisual();
    }
    
    public SqlBlock GetBlock()
    {
        return _currentBlock;
    }
}