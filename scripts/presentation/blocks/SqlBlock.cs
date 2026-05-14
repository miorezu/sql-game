using Godot;
using SQLGame.scripts.data;

public partial class SqlBlock : Button
{
    [Export] public bool ShowHint { get; set; } = true;
    [Export] public BlockType Type { get; set; }
    [Export] public string BlockValue { get; set; }
    [Export] public KeywordTypes KeywordType { get; set; } = KeywordTypes.none;

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

        if (Type == BlockType.Keyword && KeywordType == KeywordTypes.none)
            KeywordType = SqlBlockParser.ParseKeywordType(BlockValue);

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
        Text = GetBlockDisplayText();

        SelfModulate = SqlStyle.GetColorForType(Type);

        SetHint();
    }

    private string GetBlockDisplayText()
    {
        if (Type == BlockType.Keyword && KeywordType != KeywordTypes.none)
            return KeywordType.ToString();

        return BlockValue;
    }

    private void SetHint()
    {
        if (!ShowHint)
        {
            TooltipText = "";

            if (_helpLabel != null)
            {
                _helpLabel.Visible = false;
                _helpLabel.Text = "";
                _helpLabel.TooltipText = "";
                _helpLabel.MouseFilter = MouseFilterEnum.Ignore;
            }

            return;
        }

        string description = GetHintText();

        TooltipText = description;

        if (_helpLabel == null)
            return;

        _helpLabel.Visible = true;
        _helpLabel.Text = "?";
        _helpLabel.TooltipText = description;

        _helpLabel.MouseFilter = MouseFilterEnum.Ignore;
    }
    
    private string GetHintText()
    {
        return SqlKeyword.GetTooltip(Type, KeywordType, BlockValue);
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

        Control preview = Duplicate() as Control;

        if (preview != null)
            SetDragPreview(preview);

        return data;
    }
}