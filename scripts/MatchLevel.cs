using System;
using Godot;
using System.Linq;
using System.Threading.Tasks;
using SQLGame.scripts.data;

public partial class MatchLevel : Control
{
    [Export] private BlockPalette _leftItemsContainer;
    [Export] private BlockPalette _rightItemsContainer;
    
    [Export] private PackedScene _sqlBlockScene;
    [Export] private MatchBuilder _matchBuilder;
    [Export] private Button _checkButton;
    public event Action OnLevelCompleted;

    public override void _Ready()
    {
        if (_checkButton != null)
            _checkButton.Pressed += OnCheckButtonPressed;
    }
    
    public async Task LoadLevel(LevelData levelData)
    {
        GD.Print("[MatchLevel] LoadLevel called");

        if (_leftItemsContainer == null)
        {
            GD.PrintErr("[MatchLevel] _leftItemsContainer == null");
            return;
        }

        if (_rightItemsContainer == null)
        {
            GD.PrintErr("[MatchLevel] _rightItemsContainer == null");
            return;
        }

        if (_matchBuilder == null)
        {
            GD.PrintErr("[MatchLevel] _matchBuilder == null");
            return;
        }

        if (_sqlBlockScene == null)
        {
            GD.PrintErr("[MatchLevel] _sqlBlockScene == null");
            return;
        }

        var pairs = await DatabaseManager.GetMatchPairs(levelData.Code);

        GD.Print("[MatchLevel] Pairs count = " + pairs.Count);

        _matchBuilder.CreateSlots(pairs.Count);

        foreach (var pair in pairs)
        {
            var block = _sqlBlockScene.Instantiate<SqlBlock>();
            _leftItemsContainer.AddMatchBlock(block, pair.Id, pair.LeftText);
        }

        foreach (var pair in pairs.OrderBy(x => GD.Randf()))
        {
            var block = _sqlBlockScene.Instantiate<SqlBlock>();
            _rightItemsContainer.AddMatchBlock(block, pair.Id, pair.RightText);
        }
    }

    private void OnCheckButtonPressed()
    {
        UpdateSlotColor(_matchBuilder);
        if (LevelValidator.AreAllMatchPairsCorrect(_matchBuilder))
        {
            GD.Print("[Match] Рівень пройдено");
            OnLevelCompleted?.Invoke();
        }
        else
        {
            GD.Print("[Match] не пройдено");
        }
    }

    private void UpdateSlotColor(MatchBuilder matchBuilder)
    {
        foreach (MatchPairSlot slot in matchBuilder.GetChildren())
        {
            if (slot.GetLeftBlock() == null || slot.GetRightBlock() == null)
            {
                slot.ResetVisual();
                continue;
            }
            
            if (LevelValidator.AreMatchPairsCorrect(slot.GetLeftBlock(), slot.GetRightBlock()))
                slot.SetCorrectColor();
            else
                slot.SetErrorColor();
        }
    }
}