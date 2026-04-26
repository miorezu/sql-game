using Godot;
using System;
using System.Threading.Tasks;


public partial class TableLevel : Control
{
	[Export] private LevelTreeView _sourceTreeView;
	[Export] private LevelTreeView _expectedTreeView;

	[Export] private FlowContainer _sqlBlocksContainer;
	[Export] private PackedScene _sqlBlocksScene;

	public event Action OnLevelCompleted;

	private LevelData _currentLevelData;

	public async Task LoadLevel(LevelData levelData)
	{
		_currentLevelData = levelData;

		GD.Print("[TableLevel] LoadLevel called");
		GD.Print("[TableLevel] Code = " + levelData.Code);
		GD.Print("[TableLevel] SourceTableName = " + levelData.SourceTableName);
		GD.Print("[TableLevel] ExpectedTableName = " + levelData.ExpectedTableName);
		GD.Print("[TableLevel] _sourceTreeView = " + _sourceTreeView);
		GD.Print("[TableLevel] _expectedTreeView = " + _expectedTreeView);

		if (_sourceTreeView != null)
			await _sourceTreeView.LoadTable(_currentLevelData.SourceTableName);
		else
			GD.PrintErr("[TableLevel] _sourceTreeView == null");

		if (_expectedTreeView != null)
			await _expectedTreeView.LoadTable(_currentLevelData.ExpectedTableName);
		else
			GD.PrintErr("[TableLevel] _expectedTreeView == null");

		GenerateSqlBlocks(_currentLevelData);
	}

	private void GenerateSqlBlocks(LevelData levelData)
	{
		foreach (Node child in _sqlBlocksContainer.GetChildren())
			child.QueueFree();

		foreach (var query in levelData.SqlBlocks)
		{
			var block = _sqlBlocksScene.Instantiate<SqlBlock>();

			block.BlockValue = query;
			block.Type = BlockType.Value;
			block.KeywordType = KeywordTypes.none;

			_sqlBlocksContainer.AddChild(block);
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
				OnLevelCompleted?.Invoke();
		}

		return result;
	}
}
