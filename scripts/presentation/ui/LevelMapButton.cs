using Godot;

public partial class LevelMapButton : TextureButton
{
    [Export] public Color CompletedColor { get; set; } = new Color(0.2f, 0.75f, 0.35f);
    [Export] public Color CurrentColor { get; set; } = new Color(0.35f, 0.55f, 1f);
    [Export] public Color LockedColor { get; set; } = new Color(0.55f, 0.55f, 0.55f);

    [Export] public int LevelOrder { get; set; }
    [Export] public string DisplayLabelOverride { get; set; } = "";

    [Export] private Label _bestTimeLbl;

    [Export] private TextureRect _lockIcon;
    [Export] private Label _numberLevelLbl;

    public LevelStatus Status { get; private set; } = LevelStatus.Locked;

    public string LevelCode { get; set; }

    public override void _Ready()
    {
        Setup(LevelCode);
    }

    public void SetStatus(LevelStatus status)
    {
        Status = status;
        ApplyStatusVisual();
        ApplyBestTimeVisual();
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
        ApplyBestTimeVisual();
    }

    private void ApplyStatusVisual()
    {
        switch (Status)
        {
            case LevelStatus.Current:
                SelfModulate = CurrentColor;
                _lockIcon.Visible = false;
                _numberLevelLbl.Visible = true;
                break;
            case LevelStatus.Locked:
                SelfModulate = LockedColor;
                _lockIcon.Visible = true;
                _numberLevelLbl.Visible = false;
                break;
            case LevelStatus.Completed:
                SelfModulate = CompletedColor;
                _lockIcon.Visible = false;
                _numberLevelLbl.Visible = true;
                break;
            default:
                SelfModulate = new Color(0, 0, 0);
                _lockIcon.Visible = true;
                break;
        }
    }

    private void ApplyBestTimeVisual()
    {
        if (_bestTimeLbl == null)
            return;

        if (Status != LevelStatus.Completed)
        {
            _bestTimeLbl.Visible = false;
            _bestTimeLbl.Text = "";
            return;
        }

        if (SaveManager.Instance == null)
        {
            _bestTimeLbl.Visible = false;
            _bestTimeLbl.Text = "";
            return;
        }

        double bestTime = SaveManager.Instance.GetBestTimeSeconds(LevelOrder);

        if (bestTime <= 0)
        {
            _bestTimeLbl.Visible = false;
            _bestTimeLbl.Text = "";
            return;
        }

        _bestTimeLbl.Visible = true;
        _bestTimeLbl.Text = SaveManager.FormatTime(bestTime);
    }
}