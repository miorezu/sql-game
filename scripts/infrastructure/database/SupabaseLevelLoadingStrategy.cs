using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Godot;

public sealed class SupabaseLevelLoadingStrategy : ILevelLoadingStrategy
{
    private const string DefaultSupabaseUrl = "https://jyltrpmblsmwbzrdfnsj.supabase.co";
    private const string DefaultSupabaseKey = "sb_publishable_FOMUlgZSwVecfOabXTx0iw_dyoWWRiL";
    private const string DefaultLevelsTableName = "demo_levels";

    private static readonly System.Net.Http.HttpClient Client = new()
    {
        Timeout = TimeSpan.FromSeconds(5)
    };
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly string _supabaseUrl;
    private readonly string _supabaseKey;
    private readonly string _levelsTableName;

    public SupabaseLevelLoadingStrategy(
        string supabaseUrl = DefaultSupabaseUrl,
        string supabaseKey = DefaultSupabaseKey,
        string levelsTableName = DefaultLevelsTableName
    )
    {
        _supabaseUrl = supabaseUrl.TrimEnd('/');
        _supabaseKey = supabaseKey;
        _levelsTableName = levelsTableName;
    }

    public async Task<LevelData> GetLevelData(int levelOrder)
    {
        var row = await GetLevelRow(levelOrder);

        if (row == null)
            return null;

        var level = MapLevel(row);

        GD.Print($"[SupabaseLevelLoadingStrategy] Loaded level: {level.Title} (order: {level.LevelOrder})");

        return level;
    }

    public async Task<int?> GetNextLevelOrder(int currentLevelOrder)
    {
        string url =
            $"{BuildTableUrl()}?select=level_order" +
            $"&level_order=gt.{currentLevelOrder}" +
            "&order=level_order.asc" +
            "&limit=1";

        var rows = await SendGet<List<SupabaseLevelOrderRow>>(url);
        return rows?.FirstOrDefault()?.LevelOrder;
    }

    public async Task<List<MatchPairData>> GetMatchPairs(int levelOrder)
    {
        var row = await GetLevelRow(levelOrder);

        if (row?.MatchPairs == null)
            return new List<MatchPairData>();

        return row.MatchPairs
            .Select(pair => new MatchPairData
            {
                Id = pair.Id,
                LeftText = pair.LeftText,
                RightText = pair.RightText
            })
            .ToList();
    }

    private async Task<SupabaseLevelRow> GetLevelRow(int levelOrder)
    {
        string url =
            $"{BuildTableUrl()}?select=*" +
            $"&level_order=eq.{levelOrder}" +
            "&limit=1";

        var rows = await SendGet<List<SupabaseLevelRow>>(url);
        return rows?.FirstOrDefault();
    }

    private async Task<T> SendGet<T>(string url)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("apikey", _supabaseKey);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _supabaseKey);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        try
        {
            using var response = await Client.SendAsync(request);
            string responseText = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                GD.PrintErr($"[SupabaseLevelLoadingStrategy] Request failed. Code: {(int)response.StatusCode}. Body: {responseText}");
                return default;
            }

            return JsonSerializer.Deserialize<T>(responseText, JsonOptions);
        }
        catch (Exception exception)
        {
            GD.PrintErr($"[SupabaseLevelLoadingStrategy] Request error: {exception.Message}");
            return default;
        }
    }

    private string BuildTableUrl()
    {
        return $"{_supabaseUrl}/rest/v1/{_levelsTableName}";
    }

    private static LevelData MapLevel(SupabaseLevelRow row)
    {
        string levelType = DatabaseHelper.NormalizeLevelType(row.LevelType);

        return new LevelData
        {
            Id = row.Id,
            Title = row.Title,
            Description = row.Description,
            SourceTableName = row.SourceTableName ?? string.Empty,
            ExpectedTableName = row.ExpectedTableName ?? string.Empty,
            LevelOrder = row.LevelOrder,
            LevelType = levelType,
            BaseXp = row.BaseXp,
            TargetTimeSeconds = row.TargetTimeSeconds,
            SqlBlocks = MapBlocks(row.SqlBlocks),
            BuilderSolutionBlocks = levelType == "builder"
                ? row.BuilderSolutionBlocks?.ToArray() ?? Array.Empty<string>()
                : Array.Empty<string>()
        };
    }

    private static BlockData[] MapBlocks(List<SupabaseBlockRow> blocks)
    {
        if (blocks == null)
            return Array.Empty<BlockData>();

        return blocks
            .OrderBy(block => block.Order)
            .Select(block => new BlockData
            {
                Value = block.Value,
                Type = SqlBlockParser.ParseBlockType(block.Type),
                KeywordType = SqlBlockParser.ParseKeywordType(block.Value)
            })
            .ToArray();
    }

    private sealed class SupabaseLevelRow
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("source_table_name")]
        public string SourceTableName { get; set; }

        [JsonPropertyName("expected_table_name")]
        public string ExpectedTableName { get; set; }

        [JsonPropertyName("level_type")]
        public string LevelType { get; set; }

        [JsonPropertyName("level_order")]
        public int LevelOrder { get; set; }

        [JsonPropertyName("base_xp")]
        public int BaseXp { get; set; }

        [JsonPropertyName("target_time_seconds")]
        public float TargetTimeSeconds { get; set; }

        [JsonPropertyName("sql_blocks")]
        public List<SupabaseBlockRow> SqlBlocks { get; set; }

        [JsonPropertyName("builder_solution_blocks")]
        public List<string> BuilderSolutionBlocks { get; set; }

        [JsonPropertyName("match_pairs")]
        public List<SupabaseMatchPairRow> MatchPairs { get; set; }
    }

    private sealed class SupabaseLevelOrderRow
    {
        [JsonPropertyName("level_order")]
        public int LevelOrder { get; set; }
    }

    private sealed class SupabaseBlockRow
    {
        [JsonPropertyName("order")]
        public int Order { get; set; }

        [JsonPropertyName("value")]
        public string Value { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }
    }

    private sealed class SupabaseMatchPairRow
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("left_text")]
        public string LeftText { get; set; }

        [JsonPropertyName("right_text")]
        public string RightText { get; set; }
    }
}
