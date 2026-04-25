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

        bool exists = await DatabaseManager.TableExists(tableName);
        if (!exists)
        {
            GD.PrintErr($"[LevelTreeView] Таблиця '{tableName}' не існує!");
            return;
        }

        List<string> columns = await DatabaseManager.GetColumnNames(tableName);
        List<List<string>> rows = await DatabaseManager.GetRows(tableName);

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
}