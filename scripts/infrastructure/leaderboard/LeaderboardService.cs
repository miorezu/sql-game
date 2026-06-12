using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;

public partial class LeaderboardService : Node
{
    public static LeaderboardService Instance { get; private set; }

    private const int TopPlayersLimit = 100;
    private const int LeaderboardCandidateLimit = 1000;

    public event Action<bool, string> PlayerSynced;
    public event Action<List<LeaderboardEntry>> TopLoaded;
    public event Action<string> TopLoadFailed;

    public event Action<int, int> CurrentPlayerRankLoaded;
    public event Action<string> CurrentPlayerRankLoadFailed;

    public override void _Ready()
    {
        Instance = this;
    }

    public async void SyncCurrentPlayer()
    {
        await SyncCurrentPlayerAsync();
    }

    private async Task<bool> SyncCurrentPlayerAsync()
    {
        if (SaveManager.Instance == null || SaveManager.Instance.Data == null)
        {
            GD.PrintErr("[Leaderboard] SaveManager або SaveData не знайдено");
            return false;
        }

        var data = SaveManager.Instance.Data;

        if (string.IsNullOrWhiteSpace(data.PlayerId))
        {
            GD.PrintErr("[Leaderboard] PlayerId порожній");
            return false;
        }

        if (string.IsNullOrWhiteSpace(data.PlayerName))
        {
            GD.PrintErr("[Leaderboard] PlayerName порожній");
            return false;
        }

        int completedLevels = data.LastCompletedLevelOrder;
        int totalTimeSeconds = GetTotalBestTimeSeconds();
        double averageTimeSeconds = GetAverageBestTimeSeconds();
        int ratingScore = await CalculateCurrentPlayerRatingScore(data);

        var body = new Godot.Collections.Dictionary
        {
            { "player_id", data.PlayerId },
            { "player_name", data.PlayerName },
            { "completed_levels", completedLevels },
            { "total_time_seconds", totalTimeSeconds },
            { "average_time_seconds", averageTimeSeconds },
            { "xp", data.Xp },
            { "rating_score", ratingScore },
            { "updated_at", DateTime.UtcNow.ToString("o") }
        };

        string url = $"{SupabaseProjectSettings.Url.TrimEnd('/')}/rest/v1/leaderboard_players?on_conflict=player_id";

        string[] headers =
        {
            $"apikey: {SupabaseProjectSettings.Key}",
            $"Authorization: Bearer {SupabaseProjectSettings.Key}",
            "Content-Type: application/json",
            "Prefer: resolution=merge-duplicates,return=minimal"
        };

        string json = Json.Stringify(body);

        var syncCompleted = new TaskCompletionSource<bool>();

        var request = CreateRequest((result, responseCode, responseHeaders, responseBody) =>
        {
            string responseText = Encoding.UTF8.GetString(responseBody);

            if (result != (long)HttpRequest.Result.Success)
            {
                GD.PrintErr($"[Leaderboard] Network error while syncing player. Result: {result}");
                PlayerSynced?.Invoke(false, $"Network error: {result}");
                syncCompleted.TrySetResult(false);
                return;
            }

            if (responseCode >= 200 && responseCode < 300)
            {
                GD.Print("[Leaderboard] Player synced");
                PlayerSynced?.Invoke(true, "Player synced");
                syncCompleted.TrySetResult(true);
            }
            else
            {
                GD.PrintErr($"[Leaderboard] Sync failed. Code: {responseCode}. Body: {responseText}");
                PlayerSynced?.Invoke(false, responseText);
                syncCompleted.TrySetResult(false);
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
            return false;
        }

        return await syncCompleted.Task;
    }

    public async void LoadTopPlayers()
    {
        await SyncCurrentPlayerAsync();

        string url =
            $"{SupabaseProjectSettings.Url.TrimEnd('/')}/rest/v1/leaderboard_players" +
            "?select=player_id,player_name,completed_levels,average_time_seconds,xp,rating_score" +
            "&order=rating_score.desc,xp.desc,completed_levels.desc,average_time_seconds.asc" +
            $"&limit={LeaderboardCandidateLimit}";

        string[] headers =
        {
            $"apikey: {SupabaseProjectSettings.Key}",
            $"Authorization: Bearer {SupabaseProjectSettings.Key}",
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

    public async void LoadCurrentPlayerRank()
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

        bool synced = await SyncCurrentPlayerAsync();

        if (!synced)
        {
            CurrentPlayerRankLoadFailed?.Invoke("Не вдалося синхронізувати гравця");
            return;
        }

        try
        {
            LeaderboardEntry currentPlayer = await LoadCurrentPlayerEntry(currentPlayerId);

            if (currentPlayer == null)
            {
                CurrentPlayerRankLoadFailed?.Invoke("Гравця ще немає в лідерборді");
                return;
            }

            int totalPlayers = await CountLeaderboardRows("");
            int playersAhead = await CountPlayersAhead(currentPlayer);
            int rank = playersAhead + 1;

            if (totalPlayers <= 0 || rank <= 0)
            {
                CurrentPlayerRankLoadFailed?.Invoke("Не вдалося завантажити місце гравця");
                return;
            }

            CurrentPlayerRankLoaded?.Invoke(rank, totalPlayers);
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[Leaderboard] Rank load failed: {ex.Message}");
            CurrentPlayerRankLoadFailed?.Invoke("Не вдалося завантажити місце гравця");
        }
    }

    private async Task<LeaderboardEntry> LoadCurrentPlayerEntry(string currentPlayerId)
    {
        string url =
            $"{SupabaseProjectSettings.Url.TrimEnd('/')}/rest/v1/leaderboard_players" +
            "?select=player_id,player_name,completed_levels,average_time_seconds,xp,rating_score" +
            $"&player_id=eq.{EscapeFilterValue(currentPlayerId)}" +
            "&limit=1";

        var completed = new TaskCompletionSource<LeaderboardEntry>();

        var request = CreateRequest((result, responseCode, responseHeaders, responseBody) =>
        {
            string responseText = Encoding.UTF8.GetString(responseBody);

            if (result != (long)HttpRequest.Result.Success)
            {
                completed.TrySetException(new Exception($"Network error: {result}"));
                return;
            }

            if (responseCode < 200 || responseCode >= 300)
            {
                completed.TrySetException(new Exception($"HTTP {responseCode}: {responseText}"));
                return;
            }

            completed.TrySetResult(ParseSingleLeaderboardEntry(responseText));
        });

        Error error = request.Request(
            url,
            CreateSupabaseHeaders(),
            HttpClient.Method.Get
        );

        if (error != Error.Ok)
        {
            request.QueueFree();
            throw new Exception($"Request error: {error}");
        }

        return await completed.Task;
    }

    private async Task<int> CountPlayersAhead(LeaderboardEntry currentPlayer)
    {
        string scoreFilter = $"&rating_score=eq.{currentPlayer.RatingScore}";
        string xpFilter = $"{scoreFilter}&xp=eq.{currentPlayer.Xp}";
        string completedLevelsFilter = $"{xpFilter}&completed_levels=eq.{currentPlayer.CompletedLevels}";
        string averageTimeFilter = currentPlayer.AverageTimeSeconds > 0
            ? $"&average_time_seconds=eq.{FormatNumber(currentPlayer.AverageTimeSeconds)}"
            : "&average_time_seconds=eq.0";

        int playersAhead = 0;

        playersAhead += await CountLeaderboardRows($"&rating_score=gt.{currentPlayer.RatingScore}");
        playersAhead += await CountLeaderboardRows($"{scoreFilter}&xp=gt.{currentPlayer.Xp}");
        playersAhead += await CountLeaderboardRows($"{xpFilter}&completed_levels=gt.{currentPlayer.CompletedLevels}");

        if (currentPlayer.AverageTimeSeconds > 0)
        {
            playersAhead += await CountLeaderboardRows(
                $"{completedLevelsFilter}&average_time_seconds=gt.0" +
                $"&average_time_seconds=lt.{FormatNumber(currentPlayer.AverageTimeSeconds)}"
            );
        }
        else
        {
            playersAhead += await CountLeaderboardRows(
                $"{completedLevelsFilter}&average_time_seconds=gt.0"
            );
        }

        playersAhead += await CountLeaderboardRows(
            $"{completedLevelsFilter}{averageTimeFilter}" +
            $"&player_name=lt.{EscapeFilterValue(currentPlayer.PlayerName)}"
        );

        playersAhead += await CountLeaderboardRows(
            $"{completedLevelsFilter}{averageTimeFilter}" +
            $"&player_name=eq.{EscapeFilterValue(currentPlayer.PlayerName)}" +
            $"&player_id=lt.{EscapeFilterValue(currentPlayer.PlayerId)}"
        );

        return playersAhead;
    }

    private async Task<int> CountLeaderboardRows(string filters)
    {
        string url =
            $"{SupabaseProjectSettings.Url.TrimEnd('/')}/rest/v1/leaderboard_players" +
            "?select=player_id" +
            filters +
            "&limit=1";

        var completed = new TaskCompletionSource<int>();

        var request = CreateRequest((result, responseCode, responseHeaders, responseBody) =>
        {
            string responseText = Encoding.UTF8.GetString(responseBody);

            if (result != (long)HttpRequest.Result.Success)
            {
                completed.TrySetException(new Exception($"Network error: {result}"));
                return;
            }

            if (responseCode < 200 || responseCode >= 300)
            {
                completed.TrySetException(new Exception($"HTTP {responseCode}: {responseText}"));
                return;
            }

            int totalCount = ParseContentRangeTotal(responseHeaders);

            if (totalCount < 0)
            {
                completed.TrySetException(new Exception("Content-Range count was not returned."));
                return;
            }

            completed.TrySetResult(totalCount);
        });

        Error error = request.Request(
            url,
            CreateSupabaseHeaders("Prefer: count=exact"),
            HttpClient.Method.Get
        );

        if (error != Error.Ok)
        {
            request.QueueFree();
            throw new Exception($"Request error: {error}");
        }

        return await completed.Task;
    }

    private static string[] CreateSupabaseHeaders(params string[] additionalHeaders)
    {
        var headers = new List<string>
        {
            $"apikey: {SupabaseProjectSettings.Key}",
            $"Authorization: Bearer {SupabaseProjectSettings.Key}",
            "Content-Type: application/json"
        };

        headers.AddRange(additionalHeaders);
        return headers.ToArray();
    }

    private static int ParseContentRangeTotal(string[] headers)
    {
        foreach (string header in headers)
        {
            int separatorIndex = header.IndexOf(':');

            if (separatorIndex < 0)
                continue;

            string headerName = header.Substring(0, separatorIndex).Trim();

            if (!headerName.Equals("Content-Range", StringComparison.OrdinalIgnoreCase))
                continue;

            string headerValue = header.Substring(separatorIndex + 1).Trim();
            int slashIndex = headerValue.LastIndexOf('/');

            if (slashIndex < 0)
                continue;

            string totalText = headerValue[(slashIndex + 1)..].Trim();

            if (int.TryParse(totalText, NumberStyles.Integer, CultureInfo.InvariantCulture, out int total))
                return total;
        }

        return -1;
    }

    private static string EscapeFilterValue(string value)
    {
        return Uri.EscapeDataString(value ?? "");
    }

    private static string FormatNumber(double value)
    {
        return value.ToString("0.##", CultureInfo.InvariantCulture);
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

    private async Task<int> CalculateCurrentPlayerRatingScore(SaveData data)
    {
        var timeRatios = new List<double>();
        DatabaseManager.EnsureContentDatabaseInitialized();

        if (data.BestLevelTimes != null)
        {
            foreach (var pair in data.BestLevelTimes)
            {
                try
                {
                    LevelData levelData = await LevelRepository.GetLevelData(pair.Key);

                    if (levelData != null && levelData.TargetTimeSeconds > 0)
                        timeRatios.Add(pair.Value / levelData.TargetTimeSeconds);
                }
                catch (Exception ex)
                {
                    GD.PrintErr($"[Leaderboard] Failed to load target time for level {pair.Key}: {ex.Message}");
                }
            }
        }

        return LeaderboardScoreCalculator.CalculateScore(data.Xp, timeRatios);
    }

    private List<LeaderboardEntry> ParseTopPlayers(string json)
    {
        return ParseLeaderboardEntries(json)
            .Take(TopPlayersLimit)
            .ToList();
    }

    private List<LeaderboardEntry> ParseLeaderboardEntries(string json)
    {
        var result = new List<LeaderboardEntry>();

        Variant parsed = Json.ParseString(json);

        if (parsed.VariantType != Variant.Type.Array)
            return result;

        var rows = parsed.AsGodotArray();

        for (int i = 0; i < rows.Count; i++)
        {
            var row = rows[i].AsGodotDictionary();
            result.Add(ParseLeaderboardEntry(row));
        }

        result = result
            .OrderByDescending(entry => entry.RatingScore)
            .ThenByDescending(entry => entry.Xp)
            .ThenByDescending(entry => entry.CompletedLevels)
            .ThenBy(entry => entry.AverageTimeSeconds <= 0 ? double.MaxValue : entry.AverageTimeSeconds)
            .ThenBy(entry => entry.PlayerName)
            .ThenBy(entry => entry.PlayerId)
            .ToList();

        for (int i = 0; i < result.Count; i++)
            result[i].Rank = i + 1;

        return result;
    }

    private static LeaderboardEntry ParseSingleLeaderboardEntry(string json)
    {
        Variant parsed = Json.ParseString(json);

        if (parsed.VariantType != Variant.Type.Array)
            return null;

        var rows = parsed.AsGodotArray();

        if (rows.Count == 0)
            return null;

        return ParseLeaderboardEntry(rows[0].AsGodotDictionary());
    }

    private static LeaderboardEntry ParseLeaderboardEntry(Godot.Collections.Dictionary row)
    {
        int xp = row["xp"].AsInt32();
        double averageTimeSeconds = row["average_time_seconds"].AsDouble();
        int ratingScore = row.ContainsKey("rating_score")
            ? row["rating_score"].AsInt32()
            : 0;

        return new LeaderboardEntry
        {
            PlayerId = row["player_id"].AsString(),
            PlayerName = row["player_name"].AsString(),
            CompletedLevels = row["completed_levels"].AsInt32(),
            AverageTimeSeconds = averageTimeSeconds,
            Xp = xp,
            RatingScore = ratingScore > 0 || xp <= 0 ? ratingScore : xp
        };
    }
}
