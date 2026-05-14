using System.Collections.Generic;

public enum KeywordTypes
{
    SELECT,
    FROM,
    WHERE,
    INSERT,
    UPDATE,
    DELETE,
    CREATE,
    DROP,
    ALTER,
    JOIN,
    none,
}

public enum BlockType
{
    Keyword,
    Table,
    Column,
    Operator,
    Value,
    Statement
}

public static class SqlKeyword
{
    public static readonly Dictionary<KeywordTypes, string> Tooltips = new()
    {
        { KeywordTypes.SELECT, "Вибирає колонки з таблиці.\nПриклад: SELECT *" },
        { KeywordTypes.FROM, "Вказує джерело даних.\nПриклад: FROM users" },
        { KeywordTypes.WHERE, "Задає умову фільтрації.\nПриклад: WHERE id = 1" },
        { KeywordTypes.INSERT, "Додає нові записи в таблицю.\nПриклад: INSERT INTO users" },
        { KeywordTypes.UPDATE, "Оновлює наявні записи в таблиці.\nПриклад: UPDATE users SET name = 'Anna'" },
        { KeywordTypes.DELETE, "Видаляє записи з таблиці.\nПриклад: DELETE FROM users" },
        { KeywordTypes.CREATE, "Створює об'єкт бази даних.\nПриклад: CREATE TABLE users" },
        { KeywordTypes.DROP, "Видаляє об'єкт бази даних.\nПриклад: DROP TABLE users" },
        { KeywordTypes.ALTER, "Змінює структуру таблиці.\nПриклад: ALTER TABLE users" },
        { KeywordTypes.JOIN, "Об'єднує таблиці за спільним полем.\nПриклад: JOIN orders ON users.id = orders.user_id" }
    };

    public static string GetTooltip(BlockType blockType, KeywordTypes keywordType, string blockValue)
    {
        if (blockType == BlockType.Keyword &&
            Tooltips.TryGetValue(keywordType, out string keywordTooltip))
        {
            return keywordTooltip;
        }

        return blockType switch
        {
            BlockType.Table => $"Таблиця бази даних.\nНазва: {blockValue}",
            BlockType.Column => $"Колонка таблиці.\nНазва: {blockValue}",
            BlockType.Operator => $"Оператор умови або порівняння.\nПриклад: {blockValue}",
            BlockType.Value => $"Значення, яке використовується у запиті.\nПриклад: {blockValue}",
            BlockType.Statement => $"SQL-фрагмент або готова частина запиту.\nПриклад: {blockValue}",
            _ => $"{blockType}: {blockValue}"
        };
    }
}