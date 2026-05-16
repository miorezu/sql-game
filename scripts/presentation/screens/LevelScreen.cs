using System.Threading.Tasks;
using Godot;



public partial class LevelScreen : Control
{
    [Export] private TopBarUi _topBar;
    [Export] private Control _levelRoot;
    [Export] private Control _gameplayUi;

    [Export] private PackedScene _tableLevelScene;
    [Export] private PackedScene _matchLevelScene;
    [Export] private PackedScene _builderLevelScene;

    [Export] private LevelCompletePopup _levelCompletePopup;
    
    [Export] private TutorialController _tutorialController;
    
    private Control _currentLevelView;
    private LevelData _currentLevelData;
    
    private float _elapsedTime;
    private bool _isTimerRunning;
    private bool _isLevelCompleted;
    private int _wrongAttempts;
    
    public override async void _Ready()
    {
        SetupTopBar();
        SetupLevelCompletePopup();

        await LoadSelectedLevel();
    }

    public override void _Process(double delta)
    {
        if (!_isTimerRunning || _topBar == null)
            return;

        _elapsedTime += (float)delta;
        _topBar.SetTime(_elapsedTime);
    }

    private void SetupTopBar()
    {
        if (_topBar == null)
            return;

        _topBar.SetMode(TopBarUi.TopBarMode.Level);

        _topBar.HomePressed += OnSelectLevelMenuPressed;
        _topBar.RestartPressed += OnRestartPressed;
        _topBar.HintPressed += OnHintPressed;

        _topBar.SetTime(0);
    }

    private void SetupLevelCompletePopup()
    {
        if (_levelCompletePopup == null)
            return;

        _levelCompletePopup.NextLevelPressed += OnNextLevelPressed;
        _levelCompletePopup.SelectLevelPressed += OnSelectLevelMenuPressed;
    }

    private async Task LoadSelectedLevel()
    {
        var order = GameState.Instance.SelectedLevelOrder;

        _isLevelCompleted = false;
        _wrongAttempts = 0;
        SetGameplayInputEnabled(true);

        _currentLevelData = await LevelRepository.GetLevelData(order);

        if (_currentLevelData == null)
        {
            GD.PrintErr($"[LevelScreen] Рівень з order={order} не знайдено.");
            return;
        }

        ClearCurrentLevel();
        ResetTimer();

        await CreateLevelView();

        if (_currentLevelView == null)
        {
            GD.PrintErr("[LevelScreen] Level view was not created.");
            return;
        }

        StartTimer();

        await ShowAutomaticTutorial();

        SetGameplayInputEnabled(true);
    }

    private async Task ShowAutomaticTutorial()
    {
        if (_tutorialController == null)
            return;

        SetGameplayInputEnabled(false);

        await _tutorialController.ShowForLevel(
            _currentLevelData,
            _currentLevelView,
            forceShow: false
        );

        SetGameplayInputEnabled(true);
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

            case "builder":
                await CreateBuilderLevel();
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
        level.OnWrongAnswer += OnWrongAnswer;
        _levelRoot.AddChild(level);
        _currentLevelView = level;

        await level.LoadLevel(_currentLevelData);
    }

    private Task CreateBuilderLevel()
    {
        if (_builderLevelScene == null)
        {
            GD.PrintErr("[LevelScreen] BuilderLevel scene is not assigned.");
            return Task.CompletedTask;
        }

        GD.Print("[LevelScreen] Creating BuilderLevel");

        var level = _builderLevelScene.Instantiate<BuilderLevel>();

        level.OnLevelCompleted += OnLevelCompleted;
        level.OnWrongAnswer += OnWrongAnswer;
        _levelRoot.AddChild(level);
        _currentLevelView = level;

        level.LoadLevel(_currentLevelData);

        return Task.CompletedTask;
    }

    private async void OnLevelCompleted()
    {
        if (_isLevelCompleted)
            return;

        _isLevelCompleted = true;

        StopTimer();
        SetGameplayInputEnabled(false);

        GD.Print($"[LevelScreen] Рівень {_currentLevelData.LevelOrder} — '{_currentLevelData.Title}' пройдено");
        GD.Print($"[LevelScreen] Час проходження: {SaveManager.FormatTime(_elapsedTime)}");

        CompleteLevel(
            _currentLevelData,
            _elapsedTime,
            _wrongAttempts
        );

        bool hasNextLevel = await LevelRepository.HasNextLevel(_currentLevelData.LevelOrder);

        _levelCompletePopup?.ShowPopup(hasNextLevel);
    }

    private async void OnNextLevelPressed()
    {
        if (_currentLevelData == null)
            return;

        GD.Print("[LevelScreen] Next level pressed");

        var hasNext = await LevelRepository.HasNextLevel(_currentLevelData.LevelOrder);

        if (!hasNext)
        {
            GD.Print("[LevelScreen] Наступного рівня немає.");
            return;
        }

        await SceneTransitionManager.Instance.FadeWithoutChangeScene(async () =>
        {
            GameState.Instance.SelectedLevelOrder = _currentLevelData.LevelOrder + 1;

            _levelCompletePopup?.Hide();

            await LoadSelectedLevel();
        });
    }

    private void OnSelectLevelMenuPressed()
    {
        StopTimer();

        GD.Print("[LevelScreen] Select level menu pressed");

        SceneLoader.LoadSelectLevelMenu();
    }

    private async void OnRestartPressed()
    {
        if (_currentLevelData == null)
            return;

        StopTimer();

        await SceneTransitionManager.Instance.FadeWithoutChangeScene(async () =>
        {
            DatabaseManager.ResetUserDb();

            GameState.Instance.SelectedLevelOrder = _currentLevelData.LevelOrder;

            _levelCompletePopup?.Hide();

            await LoadSelectedLevel();
        });
    }

    private async void OnHintPressed()
    {
        if (_tutorialController == null)
            return;

        if (_currentLevelData == null || _currentLevelView == null)
            return;

        if (_isLevelCompleted)
            return;

        SetGameplayInputEnabled(false);

        await _tutorialController.ShowForLevel(
            _currentLevelData,
            _currentLevelView,
            forceShow: true
        );

        if (_isLevelCompleted)
            return;

        SetGameplayInputEnabled(true);
    }

    private void ClearCurrentLevel()
    {
        switch (_currentLevelView)
        {
            case null:
                return;

            case MatchLevel matchLevel:
                matchLevel.OnLevelCompleted -= OnLevelCompleted;
                matchLevel.OnWrongAnswer -= OnWrongAnswer;
                break;

            case TableLevel tableLevel:
                tableLevel.OnLevelCompleted -= OnLevelCompleted;
                break;

            case BuilderLevel builderLevel:
                builderLevel.OnLevelCompleted -= OnLevelCompleted;
                builderLevel.OnWrongAnswer -= OnWrongAnswer;
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

    private void SetGameplayInputEnabled(bool enabled)
    {
        if (_gameplayUi == null)
            return;

        _gameplayUi.ProcessMode = enabled
            ? Node.ProcessModeEnum.Inherit
            : Node.ProcessModeEnum.Disabled;
    }
    private void CompleteLevel(LevelData levelData, float elapsedSeconds, int wrongAttempts)
    {
        int calculatedXp = XpCalculator.CalculateLevelXp(
            levelData.BaseXp,
            elapsedSeconds,
            levelData.TargetTimeSeconds,
            wrongAttempts
        );

        int maxXp = XpCalculator.CalculateMaxXp(levelData.BaseXp);

        int previousBestXp = 0;

        if (SaveManager.Instance.Data.BestLevelXp.ContainsKey(levelData.LevelOrder))
        {
            previousBestXp = SaveManager.Instance.Data.BestLevelXp[levelData.LevelOrder];
        }

        int rewardXp = XpCalculator.CalculateRewardXp(
            calculatedXp,
            previousBestXp,
            maxXp
        );

        SaveManager.Instance.Data.Xp += rewardXp;

        if (calculatedXp > previousBestXp)
        {
            SaveManager.Instance.Data.BestLevelXp[levelData.LevelOrder] = calculatedXp;
        }

        if (levelData.LevelOrder > SaveManager.Instance.Data.LastCompletedLevelOrder)
        {
            SaveManager.Instance.Data.LastCompletedLevelOrder = levelData.LevelOrder;
        }

        SaveManager.Instance.Save();
        LeaderboardService.Instance?.SyncCurrentPlayer();
        GD.Print($"XP за проходження: {calculatedXp}");
        GD.Print($"Отримано XP: {rewardXp}");
    }
    private void OnWrongAnswer()
    {
        _wrongAttempts++;

        GD.Print($"[LevelScreen] Неправильних спроб: {_wrongAttempts}");
    }
}