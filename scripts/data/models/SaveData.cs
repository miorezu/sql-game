using Godot;
using Godot.Collections;


[GlobalClass]
public partial class SaveData : Resource 
{
    [Export] public string PlayerName { get; set; } = "";
    [Export] public string PlayerId { get; set; } = "";
    [Export] public int LastCompletedLevelOrder { get; set; } = 0;
    [Export] public int Xp { get; set; } = 0;
    
    [Export] public Dictionary<int, double> BestLevelTimes { get; set; } = new();
    
    [Export] public bool HasSeenWelcomeScreen { get; set; } = false;
    
    [Export] public bool TableTutorialShown { get; set; } = false;
    [Export] public bool BuilderTutorialShown { get; set; } = false;
    [Export] public bool MatchTutorialShown { get; set; } = false;
}