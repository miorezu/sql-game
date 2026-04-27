using Godot;


public partial class SqlBlock : Button
{
    [Export] public BlockType Type = BlockType.Keyword;
    [Export] public string BlockValue = "";
    [Export] public KeywordTypes KeywordType = KeywordTypes.SELECT;

    [Export] private Label _helpLabel;

    private FlowContainer HomeContainer { get; set; }

    public bool IsInBuilder { get; set; } = false;

    public int PairId { get; private set; } = -1;

    public MatchSide MatchSide { get; private set; } = MatchSide.None;

    public bool IsLeftItem => MatchSide == MatchSide.Left;
    public bool IsRightItem => MatchSide == MatchSide.Right;
    public bool IsMatchBlock => MatchSide != MatchSide.None;

    public void Init(BlockData data)
    {
        Type = data.Type;
        BlockValue = data.Value;
        KeywordType = data.KeywordType;

        PairId = data.PairId;
        MatchSide = data.MatchSide;

        IsInBuilder = false;

        UpdateUI();
    }

    public override void _Ready()
    {
        if (HomeContainer == null && GetParent() is FlowContainer flow)
            HomeContainer = flow;

        UpdateUI();
    }

    private void UpdateUI()
    {
        Text = Type == BlockType.Keyword
            ? KeywordType.ToString()
            : BlockValue;

        SelfModulate = SqlStyle.GetColorForType(Type);

        SetTooltip();
    }

    private void SetTooltip()
    {
        if (_helpLabel == null)
            return;

        if (Type == BlockType.Keyword &&
            SqlKeyword.Tooltips.TryGetValue(KeywordType, out string description))
        {
            _helpLabel.TooltipText = description;
            return;
        }

        _helpLabel.TooltipText = $"{Type}: {Text}";
    }

    public override Variant _GetDragData(Vector2 atPosition)
    {
        var data = new Godot.Collections.Dictionary
        {
            { "block", this },
            { "home_container", HomeContainer },
            { "is_in_builder", IsInBuilder },
            { "pair_id", PairId },
            { "match_side", (int)MatchSide }
        };

        Control preview = Duplicate() as Control; //копія UI-елемента для відображення під час drag
        if (preview != null)
            SetDragPreview(preview);

        return data;
    }
}