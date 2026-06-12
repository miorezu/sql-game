using System.Threading.Tasks;
using Godot;

public partial class TableTreeView : Control
{
    [Export] private Tree _tree;

    private string _currentTableName;
    private TableDataSource _currentSource = TableDataSource.Runtime;

    public async Task LoadTable(string tableName, TableDataSource source = TableDataSource.Runtime)
    {
        _currentTableName = tableName;
        _currentSource = source;

        if (_tree == null)
        {
            GD.PrintErr("[LevelTreeView] Tree node не призначено!");
            return;
        }

        _tree.Clear();

        TableSnapshot snapshot = await TablePreviewUseCase.LoadTable(tableName, source);

        if (!snapshot.Exists)
        {
            GD.PrintErr($"[LevelTreeView] Таблиця '{tableName}' не існує!");
            return;
        }

        _tree.Columns = snapshot.Columns.Count;

        for (int i = 0; i < snapshot.Columns.Count; i++)
            _tree.SetColumnTitle(i, snapshot.Columns[i]);

        _tree.ColumnTitlesVisible = true;
        _tree.HideRoot = true;

        TreeItem root = _tree.CreateItem();
        root.SetText(0, tableName);

        foreach (var row in snapshot.Rows)
        {
            TreeItem item = _tree.CreateItem(root);

            for (int col = 0; col < row.Count && col < snapshot.Columns.Count; col++)
                item.SetText(col, row[col]);
        }

        GD.Print($"[LevelTreeView] '{tableName}' завантажено: {snapshot.Rows.Count} рядків, {snapshot.Columns.Count} колонок.");
    }

    public async Task Refresh()
    {
        if (string.IsNullOrWhiteSpace(_currentTableName))
        {
            GD.PrintErr("[LevelTreeView] Немає таблиці для оновлення.");
            return;
        }

        GD.Print($"[LevelTreeView] Refresh -> {_currentTableName}");
        await LoadTable(_currentTableName, _currentSource);
    }
}
