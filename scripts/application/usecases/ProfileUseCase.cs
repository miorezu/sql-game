using System;

public static class ProfileUseCase
{
    public static ProfileSummary GetProfileSummary()
    {
        if (SaveManager.Instance == null || SaveManager.Instance.Data == null)
            return new ProfileSummary { HasData = false };

        SaveData data = SaveManager.Instance.Data;

        return new ProfileSummary
        {
            HasData = true,
            PlayerName = data.PlayerName,
            ProfileCreatedAt = data.ProfileCreatedAt,
            Xp = data.Xp,
            CompletedLevels = data.LastCompletedLevelOrder,
            TotalBestTimeSeconds = GetTotalBestTime(),
            AverageBestTimeSeconds = GetAverageBestTime(),
            BestLeaderboardRank = data.BestLeaderboardRank
        };
    }

    public static void SaveBestLeaderboardRank(int rank)
    {
        if (rank <= 0)
            return;

        if (SaveManager.Instance == null || SaveManager.Instance.Data == null)
            return;

        SaveData data = SaveManager.Instance.Data;

        if (data.BestLeaderboardRank > 0 && rank >= data.BestLeaderboardRank)
            return;

        data.BestLeaderboardRank = rank;
        SaveManager.Instance.Save();
    }

    public static string FormatTimeOrEmpty(double seconds)
    {
        if (seconds <= 0)
            return "--:--";

        return TimeFormatter.Format(seconds);
    }

    public static string FormatRank(int rank)
    {
        if (rank <= 0)
            return "--";

        return rank.ToString();
    }

    public static string FormatProfileCreatedAt(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "--";

        if (DateTime.TryParse(value, out DateTime date))
            return date.ToString("dd.MM.yyyy");

        return value;
    }

    private static double GetTotalBestTime()
    {
        SaveData data = SaveManager.Instance.Data;

        if (data.BestLevelTimes == null || data.BestLevelTimes.Count == 0)
            return 0;

        double total = 0;

        foreach (var pair in data.BestLevelTimes)
            total += pair.Value;

        return total;
    }

    private static double GetAverageBestTime()
    {
        SaveData data = SaveManager.Instance.Data;

        if (data.BestLevelTimes == null || data.BestLevelTimes.Count == 0)
            return 0;

        return GetTotalBestTime() / data.BestLevelTimes.Count;
    }
}
