public class ProfileSummary
{
    public bool HasData { get; set; }
    public string PlayerName { get; set; } = "";
    public string ProfileCreatedAt { get; set; } = "";
    public int Xp { get; set; }
    public int CompletedLevels { get; set; }
    public double TotalBestTimeSeconds { get; set; }
    public double AverageBestTimeSeconds { get; set; }
    public int BestLeaderboardRank { get; set; }
}
