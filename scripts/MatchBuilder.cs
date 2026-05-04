using Godot;

public partial class MatchBuilder : VBoxContainer
{
    [Export] private PackedScene _matchPairSlotScene;


    public void CreateSlots(int slotCount)
    {
        if (_matchPairSlotScene == null)
        {
            GD.PrintErr("[MatchBuilder] MatchPairSlotScene не задано.");
            return;
        }

        ClearSlots();

        for (int i = 0; i < slotCount; i++)
        {
            var slot = _matchPairSlotScene.Instantiate<MatchPairSlot>();
            AddChild(slot);
        }
    }

    private void ClearSlots()
    {
        foreach (Node child in GetChildren())
            child.QueueFree();
    }
}