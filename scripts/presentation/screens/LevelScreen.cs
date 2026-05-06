using System.Threading.Tasks;
using Godot;



public partial class LevelScreen : Control
{
    [Export] private TopBarUi _topBar;
    [Export] private Control _levelRoot;

    [Export] private PackedScene _tableLevelScene;
    [Export] private PackedScene _matchLevelScene;

    [Export] private LevelCompletePopup _levelCompletePopup;

    private Control _currentLevelView;
    private LevelData _currentLevelData;
    
    private float _elapsedTime;
    private bool _isTimerRunning;

    public override async void _Ready()
    {
        if (_topBar != null)
        {
            _topBar.SetMode(TopBarUi.TopBarMode.Level);
            _topBar.HomePressed += OnSelectLevelPressed;
            //_topBar.RestartPressed += OnRestartPressed;
            //_topBar.SettingsPressed += OnSettingsPressed;
            _topBar.SetTime(0);
        }
        
        if (_levelCompletePopup != null)
            _levelCompletePopup.NextLevelPressed += OnNextLevelPressed;
        
        if (_levelCompletePopup != null)
            _levelCompletePopup.SelectLevelPressed += OnSelectLevelPressed;
        await LoadSelectedLevel();
    }
    
    public override void _Process(double delta)
    {
        if (!_isTimerRunning || _topBar == null)
            return;

        _elapsedTime += (float)delta;
        _topBar.SetTime(_elapsedTime);
    }
    
    private async Task LoadSelectedLevel()
    {
        var order = GameState.Instance.SelectedLevelOrder;

        _currentLevelData = await DatabaseManager.GetLevelData(order);

        if (_currentLevelData == null)
        {
            GD.PrintErr($"[LevelScreen] Рівень з order={order} не знайдено.");
            return;
        }

        ClearCurrentLevel();
        ResetTimer();
        await CreateLevelView();
        StartTimer();
    }

    private async Task CreateLevelView()
    {
        switch (_currentLevelData.LevelType)
        {
            case "table":
                await CreateTableLevel();
                break;

            case "match":
                await CreateMatchLevel();
                break;

            default:
                GD.PrintErr($"[LevelScreen] Невідомий тип рівня: {_currentLevelData.LevelType}");
                break;
        }
    }

    private async Task CreateTableLevel()
    {
        if (_tableLevelScene == null)
        {
            GD.PrintErr("[LevelScreen] TableLevel scene is not assigned.");
            return;
        }

        GD.Print("[LevelScreen] Creating TableLevel");

        var level = _tableLevelScene.Instantiate<TableLevel>();

        level.OnLevelCompleted += OnLevelCompleted;

        _levelRoot.AddChild(level);
        _currentLevelView = level;

        await level.LoadLevel(_currentLevelData);
    }

    private async Task CreateMatchLevel()
    {
        if (_matchLevelScene == null)
        {
            GD.PrintErr("[LevelScreen] MatchLevel scene is not assigned.");
            return;
        }

        GD.Print("[LevelScreen] Creating MatchLevel");

        var level = _matchLevelScene.Instantiate<MatchLevel>();

        level.OnLevelCompleted += OnLevelCompleted;

        _levelRoot.AddChild(level);
        _currentLevelView = level;

        await level.LoadLevel(_currentLevelData);
    }

    private void OnLevelCompleted()
    {
        StopTimer();
        GD.Print($"[LevelScreen] Рівень '{_currentLevelData.Code}' пройдено");

        SaveManager.Instance.RecordLevelComplete(_currentLevelData.LevelOrder);

        _levelCompletePopup?.ShowPopup();
    }

    private async void OnNextLevelPressed()
    {
        GD.Print("[LevelScreen] Next level pressed");

        var hasNext = await DatabaseManager.HasNextLevel(_currentLevelData.LevelOrder);

        if (!hasNext)
        {
            GD.Print("[INFO] Наступного рівня немає.");
            return;
        }

        await SceneTransitionManager.Instance.FadeWithoutChangeScene(async () =>
        {
            GameState.Instance.SelectedLevelOrder = _currentLevelData.LevelOrder + 1;

            _levelCompletePopup?.Hide();

            await LoadSelectedLevel();
        });
    }

    private async void OnSelectLevelPressed()
    {
        StopTimer();
        GD.Print("[LevelScreen] All level pressed");
        SceneLoader.LoadSelectLevelMenu();
    }
    
    private void ClearCurrentLevel()
    {
        switch (_currentLevelView)
        {
            case null:
                return;

            case MatchLevel matchLevel:
                matchLevel.OnLevelCompleted -= OnLevelCompleted;
                break;

            case TableLevel tableLevel:
                tableLevel.OnLevelCompleted -= OnLevelCompleted;
                break;
        }

        _currentLevelView.QueueFree();
        _currentLevelView = null;
    }
    private void ResetTimer()
    {
        _elapsedTime = 0f;
        _isTimerRunning = false;
        _topBar?.SetTime(0);
    }

    private void StartTimer()
    {
        _isTimerRunning = true;
    }

    private void StopTimer()
    {
        _isTimerRunning = false;
    }
}