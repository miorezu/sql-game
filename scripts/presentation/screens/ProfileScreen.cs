using System;
using Godot;
using SQLGame.scripts.data;

public partial class ProfileScreen : Control
{
    [Export] private TopBarUi _topBar;
    [Export] private Label _playerNameValueLabel;
    [Export] private Label _profileCreatedAtValueLabel;
    [Export] private Label _xpValueLabel;
    [Export] private Label _completedLevelsValueLabel;
    [Export] private Label _totalBestTimeValueLabel;
    [Export] private Label _averageBestTimeValueLabel;
    [Export] private Label _currentRankValueLabel;
    [Export] private Label _bestRankValueLabel;

    public override void _Ready()
    {
        SetupTopBar();
        SubscribeLeaderboardEvents();

        Refresh();
        LoadCurrentRank();
    }

    private void SetupTopBar()
    {
        if (_topBar == null)
            return;
        _topBar.SetMode(TopBarUi.TopBarMode.Profile);
        _topBar.HomePressed += OnSelectLevelMenuPressed;
    }

    public override void _ExitTree()
    {
        UnsubscribeLeaderboardEvents();
    }

    public void Refresh()
    {
        if (SaveManager.Instance == null || SaveManager.Instance.Data == null)
        {
            SetEmptyData();
            return;
        }

        var data = SaveManager.Instance.Data;

        if (_playerNameValueLabel != null)
        {
            _playerNameValueLabel.Text = string.IsNullOrWhiteSpace(data.PlayerName)
                ? "--"
                : data.PlayerName;
        }

        if (_profileCreatedAtValueLabel != null)
            _profileCreatedAtValueLabel.Text = FormatProfileCreatedAt(data.ProfileCreatedAt);

        if (_xpValueLabel != null)
            _xpValueLabel.Text = $"{data.Xp} XP";

        if (_completedLevelsValueLabel != null)
            _completedLevelsValueLabel.Text = data.LastCompletedLevelOrder.ToString();

        if (_totalBestTimeValueLabel != null)
            _totalBestTimeValueLabel.Text = FormatTimeOrEmpty(GetTotalBestTime());

        if (_averageBestTimeValueLabel != null)
            _averageBestTimeValueLabel.Text = FormatTimeOrEmpty(GetAverageBestTime());

        if (_currentRankValueLabel != null)
            _currentRankValueLabel.Text = "завантаження...";

        if (_bestRankValueLabel != null)
            _bestRankValueLabel.Text = FormatRank(data.BestLeaderboardRank);
    }

    private void SubscribeLeaderboardEvents()
    {
        if (LeaderboardService.Instance == null)
            return;

        LeaderboardService.Instance.CurrentPlayerRankLoaded += OnCurrentPlayerRankLoaded;
        LeaderboardService.Instance.CurrentPlayerRankLoadFailed += OnCurrentPlayerRankLoadFailed;
    }

    private void UnsubscribeLeaderboardEvents()
    {
        if (LeaderboardService.Instance == null)
            return;

        LeaderboardService.Instance.CurrentPlayerRankLoaded -= OnCurrentPlayerRankLoaded;
        LeaderboardService.Instance.CurrentPlayerRankLoadFailed -= OnCurrentPlayerRankLoadFailed;
    }

    private void LoadCurrentRank()
    {
        if (LeaderboardService.Instance == null)
        {
            if (_currentRankValueLabel != null)
                _currentRankValueLabel.Text = "--";

            return;
        }

        LeaderboardService.Instance.LoadCurrentPlayerRank();
    }

    private void OnCurrentPlayerRankLoaded(int rank)
    {
        if (_currentRankValueLabel != null)
            _currentRankValueLabel.Text = FormatRank(rank);

        TrySaveBestLeaderboardRank(rank);
    }

    private void OnCurrentPlayerRankLoadFailed(string message)
    {
        if (_currentRankValueLabel != null)
            _currentRankValueLabel.Text = "--";

        GD.PrintErr($"[ProfileScreen] Не вдалося завантажити місце гравця: {message}");
    }

    private void TrySaveBestLeaderboardRank(int rank)
    {
        if (rank <= 0)
            return;

        if (SaveManager.Instance == null || SaveManager.Instance.Data == null)
            return;

        var data = SaveManager.Instance.Data;

        if (data.BestLeaderboardRank <= 0 || rank < data.BestLeaderboardRank)
        {
            data.BestLeaderboardRank = rank;
            SaveManager.Instance.Save();

            if (_bestRankValueLabel != null)
                _bestRankValueLabel.Text = FormatRank(data.BestLeaderboardRank);
        }
    }

    private void SetEmptyData()
    {
        if (_playerNameValueLabel != null)
            _playerNameValueLabel.Text = "--";

        if (_profileCreatedAtValueLabel != null)
            _profileCreatedAtValueLabel.Text = "--";

        if (_xpValueLabel != null)
            _xpValueLabel.Text = "--";

        if (_completedLevelsValueLabel != null)
            _completedLevelsValueLabel.Text = "--";

        if (_totalBestTimeValueLabel != null)
            _totalBestTimeValueLabel.Text = "--:--";

        if (_averageBestTimeValueLabel != null)
            _averageBestTimeValueLabel.Text = "--:--";

        if (_currentRankValueLabel != null)
            _currentRankValueLabel.Text = "--";

        if (_bestRankValueLabel != null)
            _bestRankValueLabel.Text = "--";
    }

    private double GetTotalBestTime()
    {
        if (SaveManager.Instance == null || SaveManager.Instance.Data == null)
            return 0;

        var bestLevelTimes = SaveManager.Instance.Data.BestLevelTimes;

        if (bestLevelTimes == null || bestLevelTimes.Count == 0)
            return 0;

        double total = 0;

        foreach (var pair in bestLevelTimes)
            total += pair.Value;

        return total;
    }

    private double GetAverageBestTime()
    {
        if (SaveManager.Instance == null || SaveManager.Instance.Data == null)
            return 0;

        var bestLevelTimes = SaveManager.Instance.Data.BestLevelTimes;

        if (bestLevelTimes == null || bestLevelTimes.Count == 0)
            return 0;

        return GetTotalBestTime() / bestLevelTimes.Count;
    }

    private string FormatTimeOrEmpty(double seconds)
    {
        if (seconds <= 0)
            return "--:--";

        return SaveManager.FormatTime(seconds);
    }

    private string FormatRank(int rank)
    {
        if (rank <= 0)
            return "--";

        return rank.ToString();
    }

    private string FormatProfileCreatedAt(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "--";

        if (DateTime.TryParse(value, out DateTime date))
            return date.ToString("dd.MM.yyyy");

        return value;
    }

    private void OnSelectLevelMenuPressed()
    {
        GD.Print("[ProfileScreen] Select level menu pressed");

        SceneLoader.LoadSelectLevelMenu();
    }
}