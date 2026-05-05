using Godot;

public partial class GameState : Node
{
    public static GameState Instance { get; private set; }
    public int SelectedLevelOrder { get; set; } = 1;

    public override void _Ready()
    {
        Instance = this;
    }
}