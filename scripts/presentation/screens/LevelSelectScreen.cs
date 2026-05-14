using Godot;
using System.Linq;

public partial class LevelSelectScreen : Control
{
    [Export] private Control _mapContent;
    [Export] private TopBarUi _topBar;
    [Export] private WelcomeScreen _welcomeScreen;

    public override void _Ready()
    {
        _topBar.SetMode(TopBarUi.TopBarMode.MainMenu);
        SetupLevelButtons();
        if (_welcomeScreen != null)
            _welcomeScreen.TryShow();
        else
            GD.PrintErr("WelcomeScreen не призначено");
    }

    public void SetupLevelButtons()
    {
        var buttons = _mapContent.GetChildren().OfType<LevelMapButton>().OrderBy(button => button.LevelOrder).ToList();
        foreach (var button in buttons)
        {
            button.Pressed += () => OnLevelMapButtonPressed(button.LevelOrder);
            var status = SaveManager.Instance.GetLevelStatus(button.LevelOrder);
            button.SetStatus(status);
            if (button.Status == LevelStatus.Locked)
            {
                button.Disabled = true;
            }
        }
    }

    private void OnLevelMapButtonPressed(int levelOrder)
    {
        GD.Print($"[LevelSelectScreen] Open level order: {levelOrder}");
        GameState.Instance.SelectedLevelOrder = levelOrder;
        SceneLoader.LoadLevelScreen();
    }
}