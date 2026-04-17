using Godot;

public partial class SqlBlock : Button
{
    [Export] public BlockType Type = BlockType.Keyword;
    [Export] public string BlockValue = "";
    [Export] public KeywordTypes KeywordType = KeywordTypes.SELECT;
    
    [Export] private Label _helpLabel;

    private FlowContainer HomeContainer { get; set; }
    public bool IsInBuilder { get; set; } = false;
    
    public void Init(BlockData data)
    {
        Type = data.Type;
        BlockValue = data.Value;
        KeywordType = data.KeywordType;
    }
    
    public override void _Ready()
    {
        if (HomeContainer == null && GetParent() is FlowContainer flow)
            HomeContainer = flow;
        UpdateUI();
    }

    private void UpdateUI()
    {
        Text = (Type == BlockType.Keyword) ? KeywordType.ToString() : BlockValue;
        
        SelfModulate = SqlStyle.GetColorForType(Type);
        
        if (_helpLabel != null)
        {
            SetTooltip();
        }
    }
    
    private void SetTooltip()
    {
        if (Type == BlockType.Keyword)
        {
            if (SqlKeyword.Tooltips.TryGetValue(KeywordType, out string description))
            {
                _helpLabel.TooltipText = description;
                return;
            }
        }
        
        _helpLabel.TooltipText = $"{Type}: {Text}";
    }

    public override Variant _GetDragData(Vector2 atPosition)
    {
        var data = new Godot.Collections.Dictionary
        {
            { "block", this }, //саме цей блок
            { "home_container", HomeContainer }, //рідний блок, щоб знати куди можна повернути блок
            { "is_in_builder", IsInBuilder }
        };

        Control preview = Duplicate() as Control; //копія UI-елемента для відображення під час drag
        if (preview != null)
            SetDragPreview(preview);

        return data;
    }
}