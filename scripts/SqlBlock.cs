using Godot;
using SQLGame.scripts.data;


public partial class SqlBlock : Button
{
    [Export] public BlockType Type { get; set; }
    [Export] public string BlockValue { get; set; }
    [Export] public KeywordTypes KeywordType { get; set; } 

    [Export] private Label _helpLabel;

    private FlowContainer HomeContainer { get; set; }

    public bool IsInBuilder { get; set; } = false;

    public int? PairId { get; private set; } 

    public MatchSide MatchSide { get; private set; } = MatchSide.None;

    private bool IsLeftItem => MatchSide == MatchSide.Left;
    private bool IsRightItem => MatchSide == MatchSide.Right;
    private bool IsMatchBlock => MatchSide != MatchSide.None;

    public void Init(BlockData data)
    {
        Type = data.Type;
        BlockValue = data.Value;
        KeywordType = data.KeywordType;

        PairId = data.PairId;
        MatchSide = data.MatchSide;

        IsInBuilder = false;

        UpdateUi();
    }

    public override void _Ready()
    {
        if (HomeContainer == null && GetParent() is FlowContainer flow)
            HomeContainer = flow;

        UpdateUi();
    }

    private void UpdateUi()
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
            { "pair_id", PairId.HasValue ? PairId.Value : -1 },
            { "match_side", (int)MatchSide }
        };

        Control preview = Duplicate() as Control; //копія UI-елемента для відображення під час drag
        if (preview != null)
            SetDragPreview(preview);

        return data;
    }
}