using Godot;



[GlobalClass]
public partial class SaveData : Resource 
{
    [Export] public string PlayerName { get; set; } = "";
    [Export] public int LastCompletedLevelOrder { get; set; } = 0;
    [Export] public int Xp { get; set; } = 0;
    
}