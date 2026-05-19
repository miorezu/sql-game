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
        LevelRepository.UseHybridDemoLoadingStrategy();

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
            var status = SaveManager.Instance.GetLevelStatus(button.LevelOrder);
            button.SetStatus(status);

            if (button.LevelOrder == LevelRepository.SupabaseDemoLevelOrder && button.Status != LevelStatus.Locked)
            {
                bool isAvailable = await IsSupabaseDemoLevelAvailable();

                if (!isAvailable)
                {
                    GD.Print("[LevelSelectScreen] Supabase demo level unavailable. Button locked.");
                    button.SetStatus(LevelStatus.Locked);
                }
            }

            if (button.Status == LevelStatus.Locked)
            {
                button.Disabled = true;
            }
        }
    }

    private async Task<bool> IsSupabaseDemoLevelAvailable()
    {
        var level = await LevelRepository.GetLevelData(LevelRepository.SupabaseDemoLevelOrder);
        return level != null;
    }

    private void OnLevelMapButtonPressed(int levelOrder)
    {
        GD.Print($"[LevelSelectScreen] Open level order: {levelOrder}");
        GameState.Instance.SelectedLevelOrder = levelOrder;
        SceneLoader.LoadLevelScreen();
    }
}