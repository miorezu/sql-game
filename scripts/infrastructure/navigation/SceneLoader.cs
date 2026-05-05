using Godot;



public static class SceneLoader
{
    private const string LevelsMenuPath = "res://scenes/LevelsMenu.tscn";
    private const string LevelScreenPath = "res://scenes/Scene.tscn";

    private static void LoadScene(string path)
    {
        var tree = Engine.GetMainLoop() as SceneTree;
        tree?.ChangeSceneToFile(path);
    }

    public static void LoadLevelsMenu()
    {
        LoadScene(LevelsMenuPath);
    }

    public static void LoadLevelScreen()
    {
        LoadScene(LevelScreenPath);
    }
}