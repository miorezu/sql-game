using Godot;

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

    public override void _ExitTree()
    {
        UnsubscribeLeaderboardEvents();
    }

    public void Refresh()
    {
        ProfileSummary summary = ProfileUseCase.GetProfileSummary();

        if (!summary.HasData)
        {
            SetEmptyData();
            return;
        }

        if (_playerNameValueLabel != null)
        {
            _playerNameValueLabel.Text = string.IsNullOrWhiteSpace(summary.PlayerName)
                ? "--"
                : summary.PlayerName;
        }

        if (_profileCreatedAtValueLabel != null)
            _profileCreatedAtValueLabel.Text = ProfileUseCase.FormatProfileCreatedAt(summary.ProfileCreatedAt);

        if (_xpValueLabel != null)
            _xpValueLabel.Text = $"{summary.Xp} XP";

        if (_completedLevelsValueLabel != null)
            _completedLevelsValueLabel.Text = summary.CompletedLevels.ToString();

        if (_totalBestTimeValueLabel != null)
            _totalBestTimeValueLabel.Text = ProfileUseCase.FormatTimeOrEmpty(summary.TotalBestTimeSeconds);

        if (_averageBestTimeValueLabel != null)
            _averageBestTimeValueLabel.Text = ProfileUseCase.FormatTimeOrEmpty(summary.AverageBestTimeSeconds);

        if (_currentRankValueLabel != null)
            _currentRankValueLabel.Text = "завантаження...";

        if (_bestRankValueLabel != null)
            _bestRankValueLabel.Text = ProfileUseCase.FormatRank(summary.BestLeaderboardRank);
    }

    private void SetupTopBar()
    {
        if (_topBar == null)
            return;

        _topBar.SetMode(TopBarUi.TopBarMode.Profile);
        _topBar.HomePressed += OnSelectLevelMenuPressed;
    }

    private void SubscribeLeaderboardEvents()
    {
        LeaderboardUseCase.SubscribeCurrentRank(
            OnCurrentPlayerRankLoaded,
            OnCurrentPlayerRankLoadFailed
        );
    }

    private void UnsubscribeLeaderboardEvents()
    {
        LeaderboardUseCase.UnsubscribeCurrentRank(
            OnCurrentPlayerRankLoaded,
            OnCurrentPlayerRankLoadFailed
        );
    }

    private void LoadCurrentRank()
    {
        if (!LeaderboardUseCase.IsAvailable)
        {
            if (_currentRankValueLabel != null)
                _currentRankValueLabel.Text = "--";

            return;
        }

        LeaderboardUseCase.LoadCurrentPlayerRank();
    }

    private void OnCurrentPlayerRankLoaded(int rank, int totalPlayers)
    {
        if (_currentRankValueLabel != null)
        {
            _currentRankValueLabel.Text = totalPlayers > 0
                ? $"{ProfileUseCase.FormatRank(rank)} з {totalPlayers}"
                : ProfileUseCase.FormatRank(rank);
        }

        ProfileUseCase.SaveBestLeaderboardRank(rank);

        if (_bestRankValueLabel != null)
        {
            ProfileSummary summary = ProfileUseCase.GetProfileSummary();
            _bestRankValueLabel.Text = ProfileUseCase.FormatRank(summary.BestLeaderboardRank);
        }
    }

    private void OnCurrentPlayerRankLoadFailed(string message)
    {
        if (_currentRankValueLabel != null)
            _currentRankValueLabel.Text = "--";

        GD.PrintErr($"[ProfileScreen] Не вдалося завантажити місце гравця: {message}");
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

    private void OnSelectLevelMenuPressed()
    {
        GD.Print("[ProfileScreen] Select level menu pressed");

        SceneLoader.LoadSelectLevelMenu();
    }
}
