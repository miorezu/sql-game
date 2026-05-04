using Godot;

public partial class MatchPairSlot : PanelContainer
{
    [Export] private MatchDropArea _leftDropArea;
    [Export] private MatchDropArea _rightDropArea;


    public SqlBlock GetLeftBlock()
    {
        return _leftDropArea?.GetBlock();
    }

    public SqlBlock GetRightBlock()
    {
        return _rightDropArea?.GetBlock();
    }

    public void ResetVisual()
    {
        Modulate = Colors.White;
    }

    public void SetCorrectColor()
    {
        Modulate = Colors.LightGreen;
    }

    public void SetErrorColor()
    {
        Modulate = Colors.IndianRed;
    }
}