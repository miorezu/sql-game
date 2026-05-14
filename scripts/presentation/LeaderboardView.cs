using System;
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
            GD.PrintErr("[LeaderboardView] Tree не призначено");
            return;
        }

        if (LeaderboardService.Instance == null)
        {
            ShowMessage("Лідерборд недоступний");
            SetRankText("Ваше місце в рейтингу: не визначено");
            return;
        }

        LeaderboardService.Instance.TopLoaded += OnTopLoaded;
        LeaderboardService.Instance.TopLoadFailed += OnTopLoadFailed;

        LoadLeaderboard();
    }

    public override void _ExitTree()
    {
        if (LeaderboardService.Instance != null)
        {
            LeaderboardService.Instance.TopLoaded -= OnTopLoaded;
            LeaderboardService.Instance.TopLoadFailed -= OnTopLoadFailed;
        }
    }

    public void LoadLeaderboard()
    {
        ShowMessage("Завантаження лідерборду...");
        SetRankText("Ваше місце в рейтингу: завантажується...");

        LeaderboardService.Instance?.LoadTopPlayers();
    }

    private void OnTopLoaded(List<LeaderboardEntry> entries)
    {
        if (entries == null || entries.Count == 0)
        {
            ShowMessage("Поки немає результатів");
            SetRankText("Ваше місце в рейтингу: поки немає результатів");
            return;
        }

        BuildTree(entries);
    }

    private void OnTopLoadFailed(string message)
    {
        ShowMessage("Немає підключення до інтернету");
        SetRankText("Ваше місце в рейтингу: -");
    }

    private void BuildTree(List<LeaderboardEntry> entries)
    {
        _tree.Clear();

        _tree.Columns = 5;
        _tree.ColumnTitlesVisible = true;
        _tree.HideRoot = true;

        _tree.SetColumnTitle(0, "№");
        _tree.SetColumnTitle(1, "Гравець");
        _tree.SetColumnTitle(2, "XP");
        _tree.SetColumnTitle(3, "Рівні");
        _tree.SetColumnTitle(4, "Сер. час");

        TreeItem root = _tree.CreateItem();

        string currentPlayerId = NormalizePlayerId(SaveManager.Instance?.Data?.PlayerId);

        int currentPlayerRank = -1;

        for (int i = 0; i < entries.Count; i++)
        {
            LeaderboardEntry entry = entries[i];

            TreeItem item = _tree.CreateItem(root);

            bool isCurrentPlayer = IsCurrentPlayer(entry, currentPlayerId);

            if (isCurrentPlayer)
                currentPlayerRank = i + 1;

            item.SetText(0, (i + 1).ToString());

            if (isCurrentPlayer)
                item.SetText(1, $"{entry.PlayerName} (ви)");
            else
                item.SetText(1, entry.PlayerName);

            item.SetText(2, entry.Xp.ToString());
            item.SetText(3, entry.CompletedLevels.ToString());
            item.SetText(4, SaveManager.FormatTime(entry.AverageTimeSeconds));

            if (isCurrentPlayer)
            {
                for (int column = 0; column < _tree.Columns; column++)
                {
                    item.SetCustomBgColor(column, new Color(0.25f, 0.45f, 0.9f, 0.35f));
                    item.SetCustomColor(column, new Color(1f, 1f, 1f));
                }
            }
        }

        SetRankPlace(currentPlayerRank, entries.Count, !string.IsNullOrWhiteSpace(currentPlayerId));
    }

    private bool IsCurrentPlayer(LeaderboardEntry entry, string currentPlayerId)
    {
        if (entry == null)
            return false;

        if (string.IsNullOrWhiteSpace(currentPlayerId))
            return false;

        string entryPlayerId = NormalizePlayerId(entry.PlayerId);

        if (string.IsNullOrWhiteSpace(entryPlayerId))
            return false;

        return string.Equals(entryPlayerId, currentPlayerId, StringComparison.Ordinal);
    }

    private static string NormalizePlayerId(string playerId)
    {
        return string.IsNullOrWhiteSpace(playerId) ? "" : playerId.Trim();
    }

    private void SetRankPlace(int rank, int totalPlayers, bool hasCurrentPlayerId)
    {
        if (_rankPlace == null)
            return;

        if (!hasCurrentPlayerId)
        {
            _rankPlace.Text = "Ваше місце в рейтингу: -";
            return;
        }

        if (rank > 0)
        {
            _rankPlace.Text = $"Ваше місце в рейтингу: {rank} із {totalPlayers}";
        }
        else
        {
            _rankPlace.Text = $"Ваше місце в рейтингу: не знайдено серед {totalPlayers} результатів";
        }
    }

    private void SetRankText(string text)
    {
        if (_rankPlace != null)
            _rankPlace.Text = text;
    }

    private void ShowMessage(string message)
    {
        _tree.Clear();

        _tree.Columns = 1;
        _tree.ColumnTitlesVisible = false;
        _tree.HideRoot = true;

        TreeItem root = _tree.CreateItem();
        TreeItem item = _tree.CreateItem(root);

        item.SetText(0, message);
    }
}