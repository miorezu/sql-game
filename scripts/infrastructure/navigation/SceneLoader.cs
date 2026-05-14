using Godot;


public static class SceneLoader
{
    private const string SelectLevelMenuPath = "res://scenes/LevelSelectScreen.tscn";
    private const string LevelScreenPath = "res://scenes/Scene.tscn";
    private const string ProfilePath = "res://scenes/ProfileScreen.tscn";

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

    public static void LoadSelectLevelMenu()
    {
        LoadScene(SelectLevelMenuPath, false);
    }

    public static void LoadLevelScreen()
    {
        LoadScene(LevelScreenPath, true);
    }

    public static void LoadProfileScreen()
    {
        LoadScene(ProfilePath, true);
    }
}