using System;
using System.Threading.Tasks;
using Godot;



public partial class TableLevel : Control
{
	[Export] private TableTreeView _sourceTreeView;
	[Export] private TableTreeView _expectedTreeView;

	[Export] private FlowContainer _sqlBlocksContainer;
	[Export] private PackedScene _sqlBlockScene;
	[Export] private QueryBuilder _queryBuilder;

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
		if (_sqlBlocksContainer == null || _sqlBlockScene == null)
		{
			GD.PrintErr("[TableLevel] SQL blocks container або scene не задані.");
			return;
		}
		foreach (Node child in _sqlBlocksContainer.GetChildren())
			child.QueueFree();

		foreach (var query in levelData.SqlBlocks)
		{
			var block = _sqlBlockScene.Instantiate<SqlBlock>();

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
	
	private async void OnCheckButtonPressed()
	{
		if (_queryBuilder == null)
		{
			GD.PrintErr("[TableLevel] QueryBuilder не призначено в Inspector.");
			return;
		}

		string sql = _queryBuilder.BuildQuery();
		GD.Print("[SQL] " + sql);

		try
		{
			QueryResult result = await ExecutePlayerSql(sql);

			if (!result.HasRows)
			{
				GD.Print($"[SQL] Query executed. Affected rows: {result.AffectedRows}");
				return;
			}

			if (result.Rows.Count == 0)
			{
				GD.Print("[SQL] Query executed, but returned 0 rows.");
				return;
			}

			foreach (var row in result.Rows)
			{
				string rowText = string.Join(" | ", row);
				GD.Print(rowText);
			}
		}
		catch (Exception e)
		{
			GD.PrintErr($"[SQL Error]: {e.Message}");
		}
	}
}