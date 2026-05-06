using Godot;



public static class SceneLoader
{
    private const string LevelsMenuPath = "res://scenes/LevelsMenu.tscn";
    private const string LevelScreenPath = "res://scenes/Scene.tscn";

    private static void LoadScene(string path, bool showLoadingText = true)
    {
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.FadeWithChangeScene(path, showLoadingText);
            return;
        }

        var tree = Engine.GetMainLoop() as SceneTree;
        tree?.ChangeSceneToFile(path);
    }

    public static void LoadLevelsMenu()
    {
        LoadScene(LevelsMenuPath, false);
    }

    public static void LoadLevelScreen()
    {
        LoadScene(LevelScreenPath, true);
    }
}