using Godot;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class LevelTreeView : Control
{
    [Export] private Tree _tree;

    private string _currentTableName;

    public async Task LoadTable(string tableName)
    {
        _currentTableName = tableName;

        if (_tree == null)
        {
            GD.PrintErr("[LevelTreeView] Tree node не призначено!");
            return;
        }

        _tree.Clear();

        bool exists = await TableExists(tableName);
        if (!exists)
        {
            GD.PrintErr($"[LevelTreeView] Таблиця '{tableName}' не існує!");
            return;
        }

        List<string> columns = await GetColumnNames(tableName);

        QueryResult queryResult = await DatabaseManager.ExecuteSql(
            $"SELECT * FROM [{tableName}]"
        );

        List<List<string>> rows = queryResult.Rows;

        _tree.Columns = columns.Count;
        for (int i = 0; i < columns.Count; i++)
            _tree.SetColumnTitle(i, columns[i]);

        _tree.ColumnTitlesVisible = true;
        _tree.HideRoot = true;

        TreeItem root = _tree.CreateItem();
        root.SetText(0, tableName);

        foreach (var row in rows)
        {
            TreeItem item = _tree.CreateItem(root);

            for (int col = 0; col < row.Count && col < columns.Count; col++)
                item.SetText(col, row[col]);
        }

        GD.Print($"[LevelTreeView] '{tableName}' завантажено: {rows.Count} рядків, {columns.Count} колонок.");
    }

    public async Task Refresh()
    {
        if (string.IsNullOrWhiteSpace(_currentTableName))
        {
            GD.PrintErr("[LevelTreeView] Немає таблиці для оновлення.");
            return;
        }

        GD.Print($"[LevelTreeView] Refresh -> {_currentTableName}");
        await LoadTable(_currentTableName);
    }

    private async Task<bool> TableExists(string tableName)
    {
        var result = await DatabaseManager.ExecuteQuery(
            $"SELECT name FROM sqlite_master WHERE type='table' AND name='{EscapeName(tableName)}'"
        );
        return result.Count > 0;
    }

    private async Task<List<string>> GetColumnNames(string tableName)
    {
        var columns = new List<string>();

        var result = await DatabaseManager.ExecuteQuery(
            $"PRAGMA table_info([{tableName}])"
        );

        foreach (var row in result)
        {
            if (row.Count > 1)
                columns.Add(row[1]);
        }

        return columns;
    }

    private static string EscapeName(string name) =>
        name.Replace("'", "''");
}