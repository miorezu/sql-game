using Godot;



public partial class SaveManager : Node
{
    public static SaveManager Instance { get; private set; }

    private const string SavePath = "user://save.tres";

    public SaveData Data { get; private set; }

    public override void _Ready()
    {
        Instance = this;
        Load();
        PrintDebugInfo();
    }

    public void Save()
    {
        if (Data == null)
        {
            GD.PrintErr("[Save] Немає даних для збереження");
            return;
        }

        var error = ResourceSaver.Save(Data, SavePath);

        if (error == Error.Ok)
        {
            GD.Print("[Save] Збережено");
        }
        else
        {
            GD.PrintErr($"[Save] Помилка збереження: {error}");
        }
    }

    public void Load()
    {
        if (ResourceLoader.Exists(SavePath))
        {
            Data = ResourceLoader.Load<SaveData>(SavePath);
            GD.Print($"[Save] Завантажено: {Data.PlayerName}, рівень {Data.LastCompletedLevelOrder}");
        }
        else
        {
            Data = new SaveData();
            Save();
            GD.Print("[Save] Створено новий файл збереження");
        }
    }

    public void RecordLevelComplete(int levelOrder)
    {
        if (levelOrder > Data.LastCompletedLevelOrder)
        {
            Data.LastCompletedLevelOrder = levelOrder;
        }
        Save();
    }
    
    public bool IsLevelUnlocked(int levelOrder)
    {
        return levelOrder <= Data.LastCompletedLevelOrder + 1;
    }
    public LevelStatus GetLevelStatus(int levelOrder)
    {
        if (levelOrder <= Data.LastCompletedLevelOrder)
        {
            return LevelStatus.Completed;
        }

        if (levelOrder == Data.LastCompletedLevelOrder + 1)
        {
            return LevelStatus.Current;
        }

        return LevelStatus.Locked;
    }
    private void PrintDebugInfo()
    {
        if (Data == null)
        {
            GD.PrintErr("[Save Debug] Data == null");
            return;
        }

        GD.Print("========== SAVE DEBUG ==========");
        GD.Print($"[Save Debug] Path: {SavePath}");
        GD.Print($"[Save Debug] Global path: {ProjectSettings.GlobalizePath(SavePath)}");
        GD.Print($"[Save Debug] PlayerName: {Data.PlayerName}");
        GD.Print($"[Save Debug] LastCompletedLevelOrder: {Data.LastCompletedLevelOrder}");
        GD.Print($"[Save Debug] Xp: {Data.Xp}");



        GD.Print("================================");
    }

}