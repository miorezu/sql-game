using Godot;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class LevelTreeView : Control
{
    [Export] private Tree _tree;

    public async Task LoadTable(string levelName)
    {
        if (_tree == null)
        {
            GD.PrintErr("[LevelTreeView] Tree node не призначено!");
            return;
        }

        _tree.Clear();

        bool exists = await TableExists(levelName);
        if (!exists)
        {
            GD.PrintErr($"[LevelTreeView] Таблиця '{levelName}' не існує!");
            return;
        }

        List<string> columns = await GetColumnNames(levelName);

        List<List<string>> rows = await DatabaseManager.ExecuteQuery(
            $"SELECT * FROM [{levelName}]"
        );


        _tree.Columns = columns.Count;
        for (int i = 0; i < columns.Count; i++)
            _tree.SetColumnTitle(i, columns[i]);

        _tree.ColumnTitlesVisible = true;
        _tree.HideRoot = true;
        TreeItem root = _tree.CreateItem();
        root.SetText(0, levelName);

        foreach (var row in rows)
        {
            TreeItem item = _tree.CreateItem(root);

            for (int col = 0; col < row.Count; col++)
            {
                if (col < columns.Count)
                    item.SetText(col, row[col]);
            }
        }

        GD.Print($"[LevelTreeView] '{levelName}' завантажено: {rows.Count} рядків, {columns.Count} колонок.");
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