using System;
using Godot;
using SQLGame.scripts.data;

public partial class SaveManager : Node
{
    public static SaveManager Instance { get; private set; }

    private const string SavePath = "user://save.dat";
    private const string SaveKeySettingPath = "app/config/save_key";

    public SaveData Data { get; private set; }

    public override void _Ready()
    {
        Instance = this;

        Load();
        EnsurePlayerId();
        EnsurePlayerName();

        if (LeaderboardService.Instance != null)
        {
            LeaderboardService.Instance.PlayerSynced += OnPlayerSynced;
            LeaderboardService.Instance.SyncCurrentPlayer();
        }
        else
        {
            GD.PrintErr("[Save] LeaderboardService.Instance == null");
        }

        PrintDebugInfo();
    }

    public void Save()
    {
        if (Data == null)
        {
            GD.PrintErr("[Save] Немає даних для збереження");
            return;
        }

        string password = GetSavePassword();

        if (string.IsNullOrWhiteSpace(password))
        {
            GD.PrintErr("[Save] Збереження скасовано: немає ключа шифрування");
            return;
        }

        using var file = FileAccess.OpenEncryptedWithPass(
            SavePath,
            FileAccess.ModeFlags.Write,
            password
        );

        if (file == null)
        {
            GD.PrintErr($"[Save] Не вдалося відкрити зашифрований файл для запису: {FileAccess.GetOpenError()}");
            return;
        }

        file.StoreVar(Data, true);

        GD.Print("[Save] Збережено у зашифрований .dat");
    }

    public void Load()
    {
        if (!FileAccess.FileExists(SavePath))
        {
            Data = new SaveData();
            Save();

            GD.Print("[Save] Створено новий зашифрований .dat файл збереження");
            return;
        }

        if (TryLoadEncryptedSave())
        {
            EnsureDataValid();
            GD.Print(
                $"[Save] Завантажено із зашифрованого .dat: {Data.PlayerName}, рівень {Data.LastCompletedLevelOrder}");
            return;
        }

        GD.PrintErr("[Save] Не вдалося завантажити зашифрований save.dat. Створюю новий.");

        Data = new SaveData();
        Save();
    }

    private bool TryLoadEncryptedSave()
    {
        string password = GetSavePassword();

        if (string.IsNullOrWhiteSpace(password))
        {
            return false;
        }

        using var file = FileAccess.OpenEncryptedWithPass(
            SavePath,
            FileAccess.ModeFlags.Read,
            password
        );

        if (file == null)
        {
            return false;
        }

        Variant loaded = file.GetVar(true);

        if (loaded.VariantType != Variant.Type.Object)
        {
            return false;
        }

        SaveData loadedData = loaded.As<SaveData>();

        if (loadedData == null)
        {
            return false;
        }

        Data = loadedData;
        return true;
    }

    private void EnsurePlayerId()
    {
        if (string.IsNullOrWhiteSpace(Data.PlayerId))
        {
            Data.PlayerId = Guid.NewGuid().ToString();
            Save();

            GD.Print($"Generated player id: {Data.PlayerId}");
        }
    }

    private void EnsurePlayerName()
    {
        if (string.IsNullOrWhiteSpace(Data.PlayerName))
        {
            string shortId = Data.PlayerId
                .Replace("-", "")
                .Substring(0, 6)
                .ToUpper();

            Data.PlayerName = $"Player-{shortId}";
            Save();
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
            Data.Xp += 10;
        }

        Save();
        LeaderboardService.Instance?.SyncCurrentPlayer();
    }

    public void RecordLevelComplete(int levelOrder, double timeSeconds)
    {
        if (levelOrder > Data.LastCompletedLevelOrder)
        {
            Data.LastCompletedLevelOrder = levelOrder;
            Data.Xp += 10;
        }

        SaveBestTime(levelOrder, timeSeconds);

        Save();
        LeaderboardService.Instance?.SyncCurrentPlayer();
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

    private void OnPlayerSynced(bool success, string message)
    {
        if (success)
        {
            GD.Print("[Leaderboard] Синхронізація успішна");
        }
        else
        {
            GD.PrintErr($"[Leaderboard] Помилка синхронізації: {message}");
        }
    }

    private static string GetSavePassword()
    {
        string password = ProjectSettings
            .GetSetting(SaveKeySettingPath, "")
            .AsString();

        if (string.IsNullOrWhiteSpace(password))
        {
            GD.PrintErr($"[Save] Не заданий ключ шифрування у ProjectSettings: {SaveKeySettingPath}");
            return "";
        }

        return password;
    }
}