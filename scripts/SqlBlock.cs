using Godot;

public partial class SqlBlock : Button
{
    [Export] public BlockType Type = BlockType.Keyword;
    [Export] public string BlockValue = "";
    
    [Export] public KeywordTypes KeywordType = KeywordTypes.SELECT;
    [Export] private Label helpLabel;

    public override void _Ready()
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        Text = (Type == BlockType.Keyword) ? KeywordType.ToString() : BlockValue;
        
        SelfModulate = SqlStyle.GetColorForType(Type);
        
        if (helpLabel != null)
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
                helpLabel.TooltipText = description;
                return;
            }
        }
        
        helpLabel.TooltipText = $"{Type}: {Text}";
    }

    public override Variant _GetDragData(Vector2 atPosition)
    {
        var data = new Godot.Collections.Dictionary {
            { "type", (int)Type },
            { "value", Text } 
        };

        Label preview = new Label { Text = Text };
        SetDragPreview(preview);
        return data;
    }
}