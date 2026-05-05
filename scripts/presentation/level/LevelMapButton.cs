using Godot;

public partial class LevelMapButton : TextureButton
{
    [Export] public Color CompletedColor { get; set; } = new Color(0.2f, 0.75f, 0.35f);
    [Export] public Color CurrentColor { get; set; } = new Color(0.35f, 0.55f, 1f);
    [Export] public Color LockedColor { get; set; } = new Color(0.55f, 0.55f, 0.55f);
    
    [Export] public int LevelOrder { get; set; }
    [Export] public string DisplayLabelOverride { get; set; } = "";

    [Export] private TextureRect _lockIcon;
    [Export] private Label _numberLevelLbl;
    
    [Export] private LevelStatus Status;
    
    public string LevelCode  { get; set; } 
    public override void _Ready()
    {
        Setup(LevelCode);
    }

    private void ApplyDisplayLabel()
    {
        if (_numberLevelLbl == null)
            return;

        _numberLevelLbl.Text = string.IsNullOrWhiteSpace(DisplayLabelOverride)
            ? LevelOrder.ToString()
            : DisplayLabelOverride;
    }
    
    public void Setup(string levelCode)
    {
        LevelCode = levelCode;

        ApplyDisplayLabel();
        ApplyStatusVisual();
    }

    private void ApplyStatusVisual()
    {
        switch (Status)
        {
            case LevelStatus.Current:
                SelfModulate = CurrentColor;
                _lockIcon.Visible = false;
                break;
            case LevelStatus.Locked:
                SelfModulate = LockedColor;
                _lockIcon.Visible = true;
                break;
            case LevelStatus.Completed:
                SelfModulate = CompletedColor;
                _lockIcon.Visible = false;
                break;
            default:
                SelfModulate = new Color(0, 0, 0);
                _lockIcon.Visible = true;
                break;
        }
    }
}
