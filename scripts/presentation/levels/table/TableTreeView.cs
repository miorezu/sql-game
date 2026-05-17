using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;



public partial class TableTreeView : Control
{
    [Export] private Tree _tree;

    private string _currentTableName;
    private DatabaseScope _currentScope = DatabaseScope.Runtime;

    public async Task LoadTable(string tableName, DatabaseScope scope = DatabaseScope.Runtime)
    {
        _currentTableName = tableName;
        _currentScope = scope;

        if (_tree == null)
        {
            GD.PrintErr("[LevelTreeView] Tree node не призначено!");
            return;
        }

        _tree.Clear();

        bool exists = await TableRepository.TableExists(tableName, scope);
        if (!exists)
        {
            GD.PrintErr($"[LevelTreeView] Таблиця '{tableName}' не існує!");
            return;
        }

        List<string> columns = await TableRepository.GetColumnNames(tableName, scope);
        List<List<string>> rows = await TableRepository.GetRows(tableName, null, scope);

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
        await LoadTable(_currentTableName, _currentScope);
    }
}