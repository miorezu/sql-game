using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class BuilderLevel : Control
{
    [Export] private Label _taskLabel;

    [Export] private FlowContainer _availableBlocksContainer;
    [Export] private FlowContainer _queryBlocksContainer;

    [Export] private PackedScene _sqlBlockScene;
    [Export] private Button _checkButton;

    [Export] private Control _blocksContainer;
    [Export] private QueryBuilder _queryBuilder;

    public Control BlocksContainer => _blocksContainer;
    public QueryBuilder QueryBuilder => _queryBuilder;
    
    public event Action OnLevelCompleted;

    private LevelData _currentLevelData;
    private List<string> _correctBlocks = new();

    private bool _isCompleted = false;

    public override void _Ready()
    {
        if (_checkButton != null)
            _checkButton.Pressed += OnCheckPressed;
    }

    public void LoadLevel(LevelData levelData)
    {
        _currentLevelData = levelData;
        _isCompleted = false;

        if (_taskLabel != null)
            _taskLabel.Text = levelData.Description ?? "";

        _correctBlocks = levelData.BuilderSolutionBlocks
            .Select(NormalizeBlockValue)
            .ToList();

        GenerateSqlBlocks(levelData);
        ClearQueryBlocks();
    }

    private void GenerateSqlBlocks(LevelData levelData)
    {
        if (_availableBlocksContainer == null || _sqlBlockScene == null)
        {
            GD.PrintErr("[BuilderLevel] Контейнер блоків або сцена SQLBlock не задані.");
            return;
        }

        foreach (Node child in _availableBlocksContainer.GetChildren())
            child.QueueFree();

        var shuffledBlocks = Shuffle(levelData.SqlBlocks);

        foreach (var blockText in shuffledBlocks)
        {
            var block = _sqlBlockScene.Instantiate<SqlBlock>();

            block.BlockValue = blockText;
            block.Type = BlockType.Value;
            block.KeywordType = KeywordTypes.none;

            ResetBlockColor(block);

            _availableBlocksContainer.AddChild(block);
        }
    }

    private void ClearQueryBlocks()
    {
        if (_queryBlocksContainer == null)
            return;

        foreach (Node child in _queryBlocksContainer.GetChildren())
            child.QueueFree();
    }

    private void OnCheckPressed()
    {
        if (_currentLevelData == null)
        {
            GD.PrintErr("[BuilderLevel] Рівень не завантажено.");
            return;
        }

        bool isCorrect = CheckBuilderAnswer();

        GD.Print($"[BuilderLevel] Player SQL: {BuildPlayerSql()}");

        if (isCorrect && !_isCompleted)
        {
            _isCompleted = true;
            OnLevelCompleted?.Invoke();
        }
    }

    private bool CheckBuilderAnswer()
    {
        if (_queryBlocksContainer == null)
        {
            GD.PrintErr("[BuilderLevel] QueryBlocksContainer не заданий.");
            return false;
        }

        var playerBlocks = GetPlayerBlocksInOrder();

        bool isFullyCorrect = true;

        for (int i = 0; i < playerBlocks.Count; i++)
        {
            var playerBlock = playerBlocks[i];
            string playerValue = NormalizeBlockValue(playerBlock.BlockValue);

            if (i >= _correctBlocks.Count)
            {
                SetWrongBlockColor(playerBlock);
                isFullyCorrect = false;
                continue;
            }

            string expectedValue = _correctBlocks[i];

            if (playerValue == expectedValue)
            {
                SetCorrectBlockColor(playerBlock);
            }
            else if (_correctBlocks.Contains(playerValue))
            {
                SetWrongPositionBlockColor(playerBlock);
                isFullyCorrect = false;
            }
            else
            {
                SetWrongBlockColor(playerBlock);
                isFullyCorrect = false;
            }
        }

        if (playerBlocks.Count != _correctBlocks.Count)
            isFullyCorrect = false;

        return isFullyCorrect;
    }

    private List<SqlBlock> GetPlayerBlocksInOrder()
    {
        var blocks = new List<SqlBlock>();

        foreach (Node child in _queryBlocksContainer.GetChildren())
        {
            if (child is SqlBlock block)
                blocks.Add(block);
        }

        return blocks;
    }

    private string BuildPlayerSql()
    {
        var parts = GetPlayerBlocksInOrder()
            .Select(block => block.BlockValue.Trim());

        return string.Join(" ", parts)
            .Replace(" ;", ";")
            .Replace("( ", "(")
            .Replace(" )", ")")
            .Replace(" ,", ",")
            .Trim();
    }

    private string NormalizeBlockValue(string value)
    {
        return value.Trim();
    }

    private List<T> Shuffle<T>(IEnumerable<T> source)
    {
        var list = source.ToList();

        var random = new RandomNumberGenerator();
        random.Randomize();

        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = random.RandiRange(0, i);
            (list[i], list[j]) = (list[j], list[i]);
        }

        return list;
    }

    private void ResetBlockColor(SqlBlock block)
    {
        block.Modulate = Colors.White;
    }

    private void SetCorrectBlockColor(SqlBlock block)
    {
        block.Modulate = new Color(0.55f, 1.0f, 0.55f);
    }

    private void SetWrongPositionBlockColor(SqlBlock block)
    {
        block.Modulate = new Color(1.0f, 0.85f, 0.35f);
    }

    private void SetWrongBlockColor(SqlBlock block)
    {
        block.Modulate = new Color(1.0f, 0.45f, 0.45f);
    }
}