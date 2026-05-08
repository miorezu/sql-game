using System;
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
            EnsureDataValid();

            GD.Print($"[Save] Завантажено: {Data.PlayerName}, рівень {Data.LastCompletedLevelOrder}");
        }
        else
        {
            Data = new SaveData();
            Save();

            GD.Print("[Save] Створено новий файл збереження");
        }
    }

    private void EnsureDataValid()
    {
        if (Data == null)
        {
            Data = new SaveData();
        }

        if (Data.BestLevelTimes == null)
        {
            Data.BestLevelTimes = new Godot.Collections.Dictionary<int, double>();
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

    public void RecordLevelComplete(int levelOrder, double timeSeconds)
    {
        if (levelOrder > Data.LastCompletedLevelOrder)
        {
            Data.LastCompletedLevelOrder = levelOrder;
        }

        SaveBestTime(levelOrder, timeSeconds);

        Save();
    }

    private void SaveBestTime(int levelOrder, double timeSeconds)
    {
        if (!Data.BestLevelTimes.ContainsKey(levelOrder))
        {
            Data.BestLevelTimes[levelOrder] = timeSeconds;
            GD.Print($"[Save] Перший найкращий час рівня {levelOrder}: {FormatTime(timeSeconds)}");
            return;
        }

        double currentBestTime = Data.BestLevelTimes[levelOrder];

        if (timeSeconds < currentBestTime)
        {
            Data.BestLevelTimes[levelOrder] = timeSeconds;
            GD.Print($"[Save] Новий найкращий час рівня {levelOrder}: {FormatTime(timeSeconds)}");
        }
        else
        {
            GD.Print($"[Save] Час проходження: {FormatTime(timeSeconds)}");
            GD.Print($"[Save] Найкращий час рівня {levelOrder} залишився: {FormatTime(currentBestTime)}");
        }
    }

    public double GetBestTimeSeconds(int levelOrder)
    {
        if (Data == null || Data.BestLevelTimes == null)
        {
            return 0;
        }

        if (!Data.BestLevelTimes.ContainsKey(levelOrder))
        {
            return 0;
        }

        return Data.BestLevelTimes[levelOrder];
    }

    public string GetBestTimeText(int levelOrder)
    {
        double bestTime = GetBestTimeSeconds(levelOrder);

        if (bestTime <= 0)
        {
            return "--:--";
        }

        return FormatTime(bestTime);
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

    public static string FormatTime(double totalSeconds)
    {
        if (totalSeconds <= 0)
        {
            return "--:--";
        }

        var time = TimeSpan.FromSeconds(totalSeconds);

        if (time.TotalHours >= 1)
        {
            return $"{(int)time.TotalHours:00}:{time.Minutes:00}:{time.Seconds:00}";
        }

        return $"{time.Minutes:00}:{time.Seconds:00}";
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

        GD.Print("[Save Debug] Best level times:");

        if (Data.BestLevelTimes != null)
        {
            foreach (var pair in Data.BestLevelTimes)
            {
                GD.Print($"[Save Debug] Level {pair.Key}: best={FormatTime(pair.Value)}");
            }
        }

        GD.Print("================================");
    }
}