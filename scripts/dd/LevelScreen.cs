using Godot;

public partial class LevelScreen : Control
{
    [Export] private LevelTreeView _sourceTreeView;
    [Export] private LevelTreeView _expectedTreeView;

    public override void _Ready()
    {
        CallDeferred(nameof(StartLoad));
    }

    private async void StartLoad()
    {
        await LoadLevel("level1");
    }

    public async System.Threading.Tasks.Task LoadLevel(string levelCode)
    {
        var levelData = await DatabaseManager.GetLevelData(levelCode);

        if (levelData == null)
        {
            GD.PrintErr($"[LevelScreen] Рівень '{levelCode}' не знайдено.");
            return;
        }

        if (_sourceTreeView != null)
            await _sourceTreeView.LoadTable(levelData.SourceTableName);

        if (_expectedTreeView != null)
            await _expectedTreeView.LoadTable(levelData.ExpectedTableName);
    }
    
}