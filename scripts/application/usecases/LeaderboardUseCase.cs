using System;
using System.Collections.Generic;

public static class LeaderboardUseCase
{
    public static bool IsAvailable => LeaderboardService.Instance != null;

    public static string CurrentPlayerId =>
        SaveManager.Instance?.Data?.PlayerId ?? "";

    public static void SubscribeTopPlayers(Action<List<LeaderboardEntry>> loaded, Action<string> failed)
    {
        if (LeaderboardService.Instance == null)
            return;

        LeaderboardService.Instance.TopLoaded += loaded;
        LeaderboardService.Instance.TopLoadFailed += failed;
    }

    public static void UnsubscribeTopPlayers(Action<List<LeaderboardEntry>> loaded, Action<string> failed)
    {
        if (LeaderboardService.Instance == null)
            return;

        LeaderboardService.Instance.TopLoaded -= loaded;
        LeaderboardService.Instance.TopLoadFailed -= failed;
    }

    public static void SubscribeCurrentRank(Action<int, int> loaded, Action<string> failed)
    {
        if (LeaderboardService.Instance == null)
            return;

        LeaderboardService.Instance.CurrentPlayerRankLoaded += loaded;
        LeaderboardService.Instance.CurrentPlayerRankLoadFailed += failed;
    }

    public static void UnsubscribeCurrentRank(Action<int, int> loaded, Action<string> failed)
    {
        if (LeaderboardService.Instance == null)
            return;

        LeaderboardService.Instance.CurrentPlayerRankLoaded -= loaded;
        LeaderboardService.Instance.CurrentPlayerRankLoadFailed -= failed;
    }

    public static void LoadTopPlayers()
    {
        LeaderboardService.Instance?.LoadTopPlayers();
    }

    public static void LoadCurrentPlayerRank()
    {
        LeaderboardService.Instance?.LoadCurrentPlayerRank();
    }

    public static string FormatAverageTime(double seconds)
    {
        if (seconds <= 0)
            return "--:--";

        return TimeFormatter.Format(seconds);
    }
}
