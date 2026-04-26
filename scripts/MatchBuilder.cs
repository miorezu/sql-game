using Godot;

public partial class MatchBuilder : VBoxContainer
{
    [Export] private PackedScene _matchPairSlotScene;


    public void CreateSlots(int _slotCount)
    {
        if (_matchPairSlotScene == null)
        {
            GD.PrintErr("[MatchBuilder] MatchPairSlotScene не задано.");
            return;
        }
        foreach (Node child in GetChildren())
            child.QueueFree();
        for (int i = 0; i < _slotCount; i++)
        {
            var slot = _matchPairSlotScene.Instantiate<MatchPairSlot>();
            AddChild(slot);
        }
    }
}