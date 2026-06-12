using Godot;
using System.Linq;
using System.Threading.Tasks;

public partial class LevelSelectScreen : Control
{
    [Export] private Control _mapContent;
    [Export] private TopBarUi _topBar;
    [Export] private WelcomeScreen _welcomeScreen;

    public override async void _Ready()
    {
        LevelSelectUseCase.InitializeLevelLoading();

        SetupTopBar();
        await SetupLevelButtons();

        if (_welcomeScreen != null)
            _welcomeScreen.TryShow();
        else
            GD.PrintErr("WelcomeScreen не призначено");
    }

    private void SetupTopBar()
    {
        if (_topBar == null)
            return;

        _topBar.SetMode(TopBarUi.TopBarMode.MainMenu);
        _topBar.ProfilePressed += OnProfilePressed;
        
    }

    private void OnProfilePressed()
    {
        SceneLoader.LoadProfileScreen();
    }

    public async Task SetupLevelButtons()
    {
        var buttons = _mapContent.GetChildren().OfType<LevelMapButton>().OrderBy(button => button.LevelOrder).ToList();
        foreach (var button in buttons)
        {
            button.Pressed += () => OnLevelMapButtonPressed(button.LevelOrder);
            LevelButtonState state = await LevelSelectUseCase.GetLevelButtonState(button.LevelOrder);
            button.SetState(state);
        }
    }

    private void OnLevelMapButtonPressed(int levelOrder)
    {
        GD.Print($"[LevelSelectScreen] Open level order: {levelOrder}");
        LevelSelectUseCase.OpenLevel(levelOrder);
        SceneLoader.LoadLevelScreen();
    }
}
