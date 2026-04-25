using Godot;
using System.Threading.Tasks;

public partial class LevelScreen : Control
{
    [Export] private LevelTreeView _sourceTreeView;
    [Export] private LevelTreeView _expectedTreeView;
    [Export] private FlowContainer _sqlBlocksContainer;
    [Export] private PackedScene _sqlBlocksScene;
    [Export] private Popup _levelCompletePopup;
    
    private LevelData _currentLevelData;
    
    public override void _Ready()
    {
        if (_levelCompletePopup != null)
            _levelCompletePopup.NextLevelPressed += OnNextLevelPressed;
        
        CallDeferred(nameof(StartLoad));
    }

    private async void StartLoad()
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

        if (_sourceTreeView != null)
            await _sourceTreeView.LoadTable(_currentLevelData.SourceTableName);

        if (_expectedTreeView != null)
            await _expectedTreeView.LoadTable(_currentLevelData.ExpectedTableName);
        
        GenerateSqlBlocks(_currentLevelData);
    }

    public void GenerateSqlBlocks(LevelData _currentLevelData)
    {
        foreach (Node child in _sqlBlocksContainer.GetChildren())
        {
            child.QueueFree();
        }
        if (_sqlBlocksContainer != null)
        {
            foreach (var query in _currentLevelData.SqlBlocks)
            {
                var blockScene = _sqlBlocksScene;
                var block = blockScene.Instantiate<SqlBlock>();
                block.BlockValue = query;
                //поміняти типи блоків
                block.Type = BlockType.Value;
                block.KeywordType = KeywordTypes.none;
                _sqlBlocksContainer.AddChild(block);
            }
        }
    }
    

    public async Task<QueryResult> ExecutePlayerSql(string sql)
    {
        var result = await DatabaseManager.ExecuteSql(sql);

        if (_sourceTreeView != null)
            await _sourceTreeView.Refresh();

        if (_currentLevelData != null)
        {
            bool isCompleted = await LevelValidator.AreTablesEqual(
                _currentLevelData.SourceTableName,
                _currentLevelData.ExpectedTableName
            );

            if (isCompleted)
            {
                OnLevelCompleted();
            }
        }

        return result;
    }

    private void OnLevelCompleted()
    {
        GD.Print($"[LevelScreen] Рівень '{_currentLevelData.Code}' пройдено: таблиці співпадають.");
        if (_levelCompletePopup != null)
            _levelCompletePopup.ShowPopup();
    }

    private async void OnNextLevelPressed()
    {
        var nextLevelCode = await DatabaseManager.GetNextLevelCode(_currentLevelData.Code);

        GD.Print("[LEVEL] nextLevelCode = " + nextLevelCode);

        if (string.IsNullOrEmpty(nextLevelCode))
        {
            GD.Print("[INFO] Наступного рівня немає.");
            //SceneLoader.LoadLevelsMenu();
            return;
        }

        await LoadLevel(nextLevelCode);
    }
}