using Godot;

public static class SceneLoader
{
    public static void LoadLevelsMenu()
    {
        var tree = Engine.GetMainLoop() as SceneTree;
        tree?.ChangeSceneToFile("res://scenes/LevelsMenu.tscn");
    }

    public static void LoadLevelScreen()
    {
        var tree = Engine.GetMainLoop() as SceneTree;
        tree?.ChangeSceneToFile("res://scenes/LevelScreen.tscn");
    }
}