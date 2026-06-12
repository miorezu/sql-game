using System.Collections.Generic;
using Godot;

public partial class LeaderboardView : Control
{
    [Export] private Tree _tree;
    [Export] private Label _rankPlace;

    public override void _Ready()
    {
        if (_tree == null)
        {
            GD.PrintErr("[LeaderboardView] Tree node не призначено.");
            return;
        }

        if (_rankPlace != null)
            _rankPlace.Text = "Ваше місце: завантаження...";

        if (!LeaderboardUseCase.IsAvailable)
        {
            ShowMessage("Лідерборд недоступний");

            if (_rankPlace != null)
                _rankPlace.Text = "Ваше місце: --";

            return;
        }

        LeaderboardUseCase.SubscribeTopPlayers(OnTopLoaded, OnTopLoadFailed);
        LeaderboardUseCase.SubscribeCurrentRank(OnCurrentPlayerRankLoaded, OnCurrentPlayerRankLoadFailed);

        LoadLeaderboard();
    }

    public override void _ExitTree()
    {
        LeaderboardUseCase.UnsubscribeTopPlayers(OnTopLoaded, OnTopLoadFailed);
        LeaderboardUseCase.UnsubscribeCurrentRank(OnCurrentPlayerRankLoaded, OnCurrentPlayerRankLoadFailed);
    }

    public void LoadLeaderboard()
    {
        ShowMessage("Завантаження лідерборду...");

        if (_rankPlace != null)
            _rankPlace.Text = "Ваше місце: завантаження...";

        LeaderboardUseCase.LoadTopPlayers();
        LeaderboardUseCase.LoadCurrentPlayerRank();
    }

    private void OnTopLoaded(List<LeaderboardEntry> entries)
    {
        SetupTree();

        if (entries == null || entries.Count == 0)
        {
            ShowMessage("Лідерборд порожній");
            return;
        }

        TreeItem root = _tree.CreateItem();

        foreach (LeaderboardEntry entry in entries)
        {
            TreeItem item = _tree.CreateItem(root);

            item.SetText(0, entry.Rank.ToString());
            item.SetText(1, entry.PlayerName);
            item.SetText(2, entry.RatingScore.ToString());
            item.SetText(3, entry.Xp.ToString());
            item.SetText(4, entry.CompletedLevels.ToString());
            item.SetText(5, LeaderboardUseCase.FormatAverageTime(entry.AverageTimeSeconds));

            if (entry.PlayerId == LeaderboardUseCase.CurrentPlayerId)
                HighlightCurrentPlayer(item);
        }
    }

    private void OnTopLoadFailed(string message)
    {
        ShowMessage(message);
    }

    private void OnCurrentPlayerRankLoaded(int rank, int totalPlayers)
    {
        if (_rankPlace != null)
            _rankPlace.Text = totalPlayers > 0
                ? $"Ваше місце: {rank} з {totalPlayers}"
                : $"Ваше місце: {rank}";
    }

    private void OnCurrentPlayerRankLoadFailed(string message)
    {
        if (_rankPlace != null)
            _rankPlace.Text = "Ваше місце: --";

        GD.PrintErr($"[LeaderboardView] Не вдалося завантажити місце гравця: {message}");
    }

    private void SetupTree()
    {
        _tree.Clear();

        _tree.Columns = 6;
        _tree.HideRoot = true;

        _tree.ScrollHorizontalEnabled = false;
        _tree.ScrollVerticalEnabled = true;

        _tree.SetColumnTitle(0, "№");
        _tree.SetColumnTitle(1, "Гравець");
        _tree.SetColumnTitle(2, "Рейтингові бали");
        _tree.SetColumnTitle(3, "XP");
        _tree.SetColumnTitle(4, "Рівні");
        _tree.SetColumnTitle(5, "Сер. час");

        _tree.SetColumnTitlesVisible(true);

        _tree.SetColumnExpand(0, false);
        _tree.SetColumnCustomMinimumWidth(0, 45);
        _tree.SetColumnClipContent(0, true);

        _tree.SetColumnExpand(1, true);
        _tree.SetColumnCustomMinimumWidth(1, 160);
        _tree.SetColumnClipContent(1, true);

        _tree.SetColumnExpand(2, false);
        _tree.SetColumnCustomMinimumWidth(2, 95);
        _tree.SetColumnClipContent(2, true);

        _tree.SetColumnExpand(3, false);
        _tree.SetColumnCustomMinimumWidth(3, 60);
        _tree.SetColumnClipContent(3, true);

        _tree.SetColumnExpand(4, false);
        _tree.SetColumnCustomMinimumWidth(4, 70);
        _tree.SetColumnClipContent(4, true);

        _tree.SetColumnExpand(5, false);
        _tree.SetColumnCustomMinimumWidth(5, 90);
        _tree.SetColumnClipContent(5, true);
    }

    private void ShowMessage(string message)
    {
        _tree.Clear();

        _tree.Columns = 1;
        _tree.HideRoot = true;
        _tree.SetColumnTitlesVisible(false);

        TreeItem root = _tree.CreateItem();
        TreeItem item = _tree.CreateItem(root);
        item.SetText(0, message);
    }

    private void HighlightCurrentPlayer(TreeItem item)
    {
        var color = new Color(0.35f, 0.75f, 1f);

        for (int column = 0; column < 6; column++)
            item.SetCustomColor(column, color);
    }
}
