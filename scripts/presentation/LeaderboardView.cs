using System.Collections.Generic;
using Godot;
using SQLGame.scripts.data;

public partial class LeaderboardView : Control
{
    [Export] private Tree _tree;
    [Export] private Label _rankPlace;

    public override void _Ready()
    {
        if (_tree == null)
        {
            GD.PrintErr("[LeaderboardView] Tree node не призначено!");
            return;
        }

        if (_rankPlace != null)
            _rankPlace.Text = "Ваше місце: завантаження...";

        if (LeaderboardService.Instance == null)
        {
            ShowMessage("Лідерборд недоступний");

            if (_rankPlace != null)
                _rankPlace.Text = "Ваше місце: --";

            return;
        }

        LeaderboardService.Instance.TopLoaded += OnTopLoaded;
        LeaderboardService.Instance.TopLoadFailed += OnTopLoadFailed;

        LeaderboardService.Instance.CurrentPlayerRankLoaded += OnCurrentPlayerRankLoaded;
        LeaderboardService.Instance.CurrentPlayerRankLoadFailed += OnCurrentPlayerRankLoadFailed;

        LoadLeaderboard();
    }

    public override void _ExitTree()
    {
        if (LeaderboardService.Instance != null)
        {
            LeaderboardService.Instance.TopLoaded -= OnTopLoaded;
            LeaderboardService.Instance.TopLoadFailed -= OnTopLoadFailed;

            LeaderboardService.Instance.CurrentPlayerRankLoaded -= OnCurrentPlayerRankLoaded;
            LeaderboardService.Instance.CurrentPlayerRankLoadFailed -= OnCurrentPlayerRankLoadFailed;
        }
    }

    public void LoadLeaderboard()
    {
        ShowMessage("Завантаження лідерборду...");

        if (_rankPlace != null)
            _rankPlace.Text = "Ваше місце: завантаження...";

        LeaderboardService.Instance?.LoadTopPlayers();
        LeaderboardService.Instance?.LoadCurrentPlayerRank();
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
            item.SetText(2, entry.Xp.ToString());
            item.SetText(3, entry.CompletedLevels.ToString());
            item.SetText(4, FormatAverageTime(entry.AverageTimeSeconds));

            if (SaveManager.Instance != null &&
                SaveManager.Instance.Data != null &&
                entry.PlayerId == SaveManager.Instance.Data.PlayerId)
            {
                item.SetCustomColor(0, new Color(0.35f, 0.75f, 1f));
                item.SetCustomColor(1, new Color(0.35f, 0.75f, 1f));
                item.SetCustomColor(2, new Color(0.35f, 0.75f, 1f));
                item.SetCustomColor(3, new Color(0.35f, 0.75f, 1f));
                item.SetCustomColor(4, new Color(0.35f, 0.75f, 1f));
            }
        }
    }

    private void OnTopLoadFailed(string message)
    {
        ShowMessage(message);
    }

    private void OnCurrentPlayerRankLoaded(int rank)
    {
        if (_rankPlace != null)
            _rankPlace.Text = $"Ваше місце: {rank}";
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

        _tree.Columns = 5;
        _tree.HideRoot = true;

        _tree.ScrollHorizontalEnabled = false;
        _tree.ScrollVerticalEnabled = true;

        _tree.SetColumnTitle(0, "№");
        _tree.SetColumnTitle(1, "Гравець");
        _tree.SetColumnTitle(2, "XP");
        _tree.SetColumnTitle(3, "Рівні");
        _tree.SetColumnTitle(4, "Сер. час");

        _tree.SetColumnTitlesVisible(true);

        _tree.SetColumnExpand(0, false);
        _tree.SetColumnCustomMinimumWidth(0, 45);
        _tree.SetColumnClipContent(0, true);

        _tree.SetColumnExpand(1, true);
        _tree.SetColumnCustomMinimumWidth(1, 160);
        _tree.SetColumnClipContent(1, true);

        _tree.SetColumnExpand(2, false);
        _tree.SetColumnCustomMinimumWidth(2, 60);
        _tree.SetColumnClipContent(2, true);

        _tree.SetColumnExpand(3, false);
        _tree.SetColumnCustomMinimumWidth(3, 70);
        _tree.SetColumnClipContent(3, true);

        _tree.SetColumnExpand(4, false);
        _tree.SetColumnCustomMinimumWidth(4, 90);
        _tree.SetColumnClipContent(4, true);
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

    private string FormatAverageTime(double seconds)
    {
        if (seconds <= 0)
            return "--:--";

        return SaveManager.FormatTime(seconds);
    }
}