using Godot;
using System.Threading.Tasks;
using SQLGame.scripts.data;


public partial class LevelScreen : Control
{
    [Export] private Control _levelRoot;

    [Export] private PackedScene _tableLevelScene;
    [Export] private PackedScene _matchLevelScene;

    [Export] private LevelCompletePopup _levelCompletePopup;

    private Control _currentLevelView;
    private LevelData _currentLevelData;

    public override void _Ready()
    {
        if (_levelCompletePopup != null)
            _levelCompletePopup.NextLevelPressed += OnNextLevelPressed;

        CallDeferred(nameof(LoadFirstLevel));
    }

    private async void LoadFirstLevel()
    {
        await LoadLevel("level1");
    }

    public async Task LoadLevel(string levelCode)
    {
        _currentLevelData = await DatabaseManager.GetLevelData(levelCode);

        if (_currentLevelData == null)
        {
            GD.PrintErr($"[LevelScreen] Рівень '{levelCode}' не знайдено.");
            return;
        }

        ClearCurrentLevel();

        switch (_currentLevelData.LevelType)
        {
            case "table":
                await CreateTableLevel();
                break;

            case "match":
                await CreateMatchLevel();
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

        var level = _matchLevelScene.Instantiate<MatchLevel>();

        level.OnLevelCompleted += OnLevelCompleted;

        _levelRoot.AddChild(level);
        _currentLevelView = level;

        await level.LoadLevel(_currentLevelData);
    }

    private void OnLevelCompleted()
    {
        GD.Print($"[LevelScreen] Рівень '{_currentLevelData.Code}' пройдено");

        _levelCompletePopup?.ShowPopup();
    }

    private async void OnNextLevelPressed()
    {
        GD.Print("[CLICK] Next level pressed");

        var nextLevelCode = await DatabaseManager.GetNextLevelCode(_currentLevelData.Code);

        if (string.IsNullOrEmpty(nextLevelCode))
        {
            GD.Print("[INFO] Наступного рівня немає.");
            return;
        }

        await LoadLevel(nextLevelCode);
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
}