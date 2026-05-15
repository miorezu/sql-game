namespace SQLGame.scripts.data;

using System;
using System.Collections.Generic;
using System.Text;
using Godot;

public partial class LeaderboardService : Node
{
    public static LeaderboardService Instance { get; private set; }

    [Export] public string SupabaseUrl { get; set; } = "https://jyltrpmblsmwbzrdfnsj.supabase.co";
    [Export] public string SupabaseKey { get; set; } = "sb_publishable_FOMUlgZSwVecfOabXTx0iw_dyoWWRiL";
private const int TopPlayersLimit = 100;

    public event Action<bool, string> PlayerSynced;
    public event Action<List<LeaderboardEntry>> TopLoaded;
    public event Action<string> TopLoadFailed;

    public event Action<int> CurrentPlayerRankLoaded;
    public event Action<string> CurrentPlayerRankLoadFailed;

    public override void _Ready()
    {
        Instance = this;
    }

    public void SyncCurrentPlayer()
    {
        if (SaveManager.Instance == null || SaveManager.Instance.Data == null)
        {
            GD.PrintErr("[Leaderboard] SaveManager або SaveData не знайдено");
            return;
        }

        var data = SaveManager.Instance.Data;

        if (string.IsNullOrWhiteSpace(data.PlayerId))
        {
            GD.PrintErr("[Leaderboard] PlayerId порожній");
            return;
        }

        if (string.IsNullOrWhiteSpace(data.PlayerName))
        {
            GD.PrintErr("[Leaderboard] PlayerName порожній");
            return;
        }

        int completedLevels = data.LastCompletedLevelOrder;
        int totalTimeSeconds = GetTotalBestTimeSeconds();
        double averageTimeSeconds = GetAverageBestTimeSeconds();

        var body = new Godot.Collections.Dictionary
        {
            { "player_id", data.PlayerId },
            { "player_name", data.PlayerName },
            { "completed_levels", completedLevels },
            { "total_time_seconds", totalTimeSeconds },
            { "average_time_seconds", averageTimeSeconds },
            { "xp", data.Xp },
            { "updated_at", DateTime.UtcNow.ToString("o") }
        };

        string url = $"{SupabaseUrl.TrimEnd('/')}/rest/v1/leaderboard_players?on_conflict=player_id";

        string[] headers =
        {
            $"apikey: {SupabaseKey}",
            $"Authorization: Bearer {SupabaseKey}",
            "Content-Type: application/json",
            "Prefer: resolution=merge-duplicates,return=minimal"
        };

        string json = Json.Stringify(body);

        var request = CreateRequest((result, responseCode, responseHeaders, responseBody) =>
        {
            string responseText = Encoding.UTF8.GetString(responseBody);

            if (result != (long)HttpRequest.Result.Success)
            {
                GD.PrintErr($"[Leaderboard] Network error while syncing player. Result: {result}");
                PlayerSynced?.Invoke(false, $"Network error: {result}");
                return;
            }

            if (responseCode >= 200 && responseCode < 300)
            {
                GD.Print("[Leaderboard] Player synced");
                PlayerSynced?.Invoke(true, "Player synced");
            }
            else
            {
                GD.PrintErr($"[Leaderboard] Sync failed. Code: {responseCode}. Body: {responseText}");
                PlayerSynced?.Invoke(false, responseText);
            }
        });

        Error error = request.Request(
            url,
            headers,
            HttpClient.Method.Post,
            json
        );

        if (error != Error.Ok)
        {
            GD.PrintErr($"[Leaderboard] Request error: {error}");
            PlayerSynced?.Invoke(false, error.ToString());
            request.QueueFree();
        }
    }

    public void LoadTopPlayers()
    {
        string url =
            $"{SupabaseUrl.TrimEnd('/')}/rest/v1/leaderboard_players" +
            "?select=player_id,player_name,completed_levels,average_time_seconds,xp" +
            "&order=xp.desc,completed_levels.desc,average_time_seconds.asc" +
            $"&limit={TopPlayersLimit}";

        string[] headers =
        {
            $"apikey: {SupabaseKey}",
            $"Authorization: Bearer {SupabaseKey}",
            "Content-Type: application/json"
        };

        var request = CreateRequest((result, responseCode, responseHeaders, responseBody) =>
        {
            string responseText = Encoding.UTF8.GetString(responseBody);

            if (result != (long)HttpRequest.Result.Success)
            {
                GD.PrintErr($"[Leaderboard] Network error. Result: {result}");
                TopLoadFailed?.Invoke("Немає підключення до інтернету");
                return;
            }

            if (responseCode < 200 || responseCode >= 300)
            {
                GD.PrintErr($"[Leaderboard] Load top failed. Code: {responseCode}. Body: {responseText}");
                TopLoadFailed?.Invoke("Немає підключення до інтернету");
                return;
            }

            var entries = ParseTopPlayers(responseText);
            TopLoaded?.Invoke(entries);
        });

        Error error = request.Request(
            url,
            headers,
            HttpClient.Method.Get
        );

        if (error != Error.Ok)
        {
            GD.PrintErr($"[Leaderboard] Request error: {error}");
            TopLoadFailed?.Invoke("Немає підключення до інтернету");
            request.QueueFree();
        }
    }

    public void LoadCurrentPlayerRank()
    {
        if (SaveManager.Instance == null || SaveManager.Instance.Data == null)
        {
            CurrentPlayerRankLoadFailed?.Invoke("Дані гравця не знайдено");
            return;
        }

        string currentPlayerId = SaveManager.Instance.Data.PlayerId;

        if (string.IsNullOrWhiteSpace(currentPlayerId))
        {
            CurrentPlayerRankLoadFailed?.Invoke("PlayerId порожній");
            return;
        }

        string url =
            $"{SupabaseUrl.TrimEnd('/')}/rest/v1/leaderboard_players" +
            "?select=player_id" +
            "&order=xp.desc,completed_levels.desc,average_time_seconds.asc";

        string[] headers =
        {
            $"apikey: {SupabaseKey}",
            $"Authorization: Bearer {SupabaseKey}",
            "Content-Type: application/json"
        };

        var request = CreateRequest((result, responseCode, responseHeaders, responseBody) =>
        {
            string responseText = Encoding.UTF8.GetString(responseBody);

            if (result != (long)HttpRequest.Result.Success)
            {
                GD.PrintErr($"[Leaderboard] Rank network error. Result: {result}");
                CurrentPlayerRankLoadFailed?.Invoke("Немає підключення до інтернету");
                return;
            }

            if (responseCode < 200 || responseCode >= 300)
            {
                GD.PrintErr($"[Leaderboard] Rank load failed. Code: {responseCode}. Body: {responseText}");
                CurrentPlayerRankLoadFailed?.Invoke("Не вдалося завантажити місце гравця");
                return;
            }

            int rank = ParseCurrentPlayerRank(responseText, currentPlayerId);

            if (rank <= 0)
            {
                CurrentPlayerRankLoadFailed?.Invoke("Гравця ще немає в лідерборді");
                return;
            }

            CurrentPlayerRankLoaded?.Invoke(rank);
        });

        Error error = request.Request(
            url,
            headers,
            HttpClient.Method.Get
        );

        if (error != Error.Ok)
        {
            GD.PrintErr($"[Leaderboard] Rank request error: {error}");
            CurrentPlayerRankLoadFailed?.Invoke("Не вдалося виконати запит");
            request.QueueFree();
        }
    }

    private int ParseCurrentPlayerRank(string json, string currentPlayerId)
    {
        Variant parsed = Json.ParseString(json);

        if (parsed.VariantType != Variant.Type.Array)
            return -1;

        var rows = parsed.AsGodotArray();

        for (int i = 0; i < rows.Count; i++)
        {
            var row = rows[i].AsGodotDictionary();

            string playerId = row["player_id"].AsString();

            if (playerId == currentPlayerId)
                return i + 1;
        }

        return -1;
    }

    private HttpRequest CreateRequest(Action<long, long, string[], byte[]> callback)
    {
        var request = new HttpRequest();
        AddChild(request);

        request.RequestCompleted += (result, responseCode, headers, body) =>
        {
            callback(result, responseCode, headers, body);
            request.QueueFree();
        };

        return request;
    }

    private int GetTotalBestTimeSeconds()
    {
        var data = SaveManager.Instance.Data;

        if (data.BestLevelTimes == null || data.BestLevelTimes.Count == 0)
            return 0;

        double total = 0;

        foreach (var pair in data.BestLevelTimes)
        {
            total += pair.Value;
        }

        return (int)Math.Round(total);
    }

    private double GetAverageBestTimeSeconds()
    {
        var data = SaveManager.Instance.Data;

        if (data.BestLevelTimes == null || data.BestLevelTimes.Count == 0)
            return 0;

        double total = 0;

        foreach (var pair in data.BestLevelTimes)
        {
            total += pair.Value;
        }

        return Math.Round(total / data.BestLevelTimes.Count, 2);
    }

    private List<LeaderboardEntry> ParseTopPlayers(string json)
    {
        var result = new List<LeaderboardEntry>();

        Variant parsed = Json.ParseString(json);

        if (parsed.VariantType != Variant.Type.Array)
            return result;

        var rows = parsed.AsGodotArray();

        for (int i = 0; i < rows.Count; i++)
        {
            var row = rows[i].AsGodotDictionary();

            result.Add(new LeaderboardEntry
            {
                Rank = i + 1,
                PlayerId = row["player_id"].AsString(),
                PlayerName = row["player_name"].AsString(),
                CompletedLevels = row["completed_levels"].AsInt32(),
                AverageTimeSeconds = row["average_time_seconds"].AsDouble(),
                Xp = row["xp"].AsInt32()
            });
        }

        return result;
    }
}

public class LeaderboardEntry
{
    public int Rank { get; set; }
    public string PlayerId { get; set; }
    public string PlayerName { get; set; }
    public int CompletedLevels { get; set; }
    public double AverageTimeSeconds { get; set; }
    public int Xp { get; set; }
}