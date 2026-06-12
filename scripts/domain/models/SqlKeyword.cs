using System;
using System.Collections.Generic;

public enum KeywordTypes
{
    SELECT = 0,
    FROM = 1,
    WHERE = 2,
    INSERT = 3,
    UPDATE = 4,
    DELETE = 5,
    CREATE = 6,
    DROP = 7,
    ALTER = 8,
    JOIN = 9,
    none = 10,
    DISTINCT = 11,
    AS = 12,
    ON = 13,
    INNER = 14,
    LEFT = 15,
    RIGHT = 16,
    FULL = 17,
    CROSS = 18,
    OUTER = 19,
    INNER_JOIN = 20,
    LEFT_JOIN = 21,
    RIGHT_JOIN = 22,
    FULL_JOIN = 23,
    CROSS_JOIN = 24,
    GROUP = 25,
    BY = 26,
    GROUP_BY = 27,
    HAVING = 28,
    ORDER = 29,
    ORDER_BY = 30,
    ASC = 31,
    DESC = 32,
    LIMIT = 33,
    OFFSET = 34,
    INTO = 35,
    INSERT_INTO = 36,
    VALUES = 37,
    SET = 38,
    UPDATE_SET = 39,
    TABLE = 40,
    CREATE_TABLE = 41,
    ALTER_TABLE = 42,
    DROP_TABLE = 43,
    DELETE_FROM = 44,
    AND = 45,
    OR = 46,
    NOT = 47,
    IS = 48,
    IS_NOT = 49,
    NULL = 50,
    IN = 51,
    LIKE = 52,
    BETWEEN = 53,
    EXISTS = 54,
    UNION = 55,
    UNION_ALL = 56,
    ALL = 57,
    CASE = 58,
    WHEN = 59,
    THEN = 60,
    ELSE = 61,
    END = 62,
    COUNT = 63,
    SUM = 64,
    AVG = 65,
    MIN = 66,
    MAX = 67,
    PRIMARY = 68,
    KEY = 69,
    PRIMARY_KEY = 70,
    FOREIGN = 71,
    FOREIGN_KEY = 72,
    REFERENCES = 73,
    INTEGER = 74,
    TEXT = 75,
    REAL = 76,
    BLOB = 77,
    UNIQUE = 78,
    DEFAULT = 79,
    CHECK = 80,
    AUTOINCREMENT = 81,
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
        { KeywordTypes.SELECT, BuildTooltip("Вибирає колонки, вирази або всі дані з таблиці.", "початок запиту", "FROM, DISTINCT, назви колонок або *", "SELECT name, age") },
        { KeywordTypes.FROM, BuildTooltip("Вказує таблицю або підзапит, з якого беруться дані.", "SELECT або DELETE", "назва таблиці, JOIN або WHERE", "FROM users") },
        { KeywordTypes.WHERE, BuildTooltip("Задає умову фільтрації рядків до групування та сортування.", "FROM, JOIN або DELETE FROM", "умова з колонкою, оператором і значенням", "WHERE age >= 18") },
        { KeywordTypes.INSERT, BuildTooltip("Починає команду додавання нового рядка до таблиці.", "початок запиту", "INTO", "INSERT INTO users (name) VALUES ('Anna')") },
        { KeywordTypes.UPDATE, BuildTooltip("Починає команду зміни вже наявних рядків у таблиці.", "початок запиту", "назва таблиці та SET", "UPDATE users SET name = 'Anna'") },
        { KeywordTypes.DELETE, BuildTooltip("Починає команду видалення рядків з таблиці.", "початок запиту", "FROM", "DELETE FROM users WHERE id = 1") },
        { KeywordTypes.CREATE, BuildTooltip("Починає команду створення нового об'єкта бази даних.", "початок запиту", "TABLE або інший тип об'єкта", "CREATE TABLE users (id INTEGER)") },
        { KeywordTypes.DROP, BuildTooltip("Починає команду повного видалення об'єкта бази даних.", "початок запиту", "TABLE та назва таблиці", "DROP TABLE old_users") },
        { KeywordTypes.ALTER, BuildTooltip("Починає команду зміни структури вже наявної таблиці.", "початок запиту", "TABLE, назва таблиці та дія", "ALTER TABLE users ADD COLUMN email TEXT") },
        { KeywordTypes.JOIN, BuildTooltip("Приєднує іншу таблицю до результату за спільною умовою.", "FROM або інша JOIN-конструкція", "назва таблиці та ON", "JOIN orders ON users.id = orders.user_id") },
        { KeywordTypes.DISTINCT, BuildTooltip("Прибирає дублікати з результату SELECT.", "SELECT", "назви колонок або вирази", "SELECT DISTINCT city FROM users") },
        { KeywordTypes.AS, BuildTooltip("Задає псевдонім для колонки, виразу або таблиці.", "назва колонки, вираз або таблиця", "новий короткий псевдонім", "SELECT name AS user_name") },
        { KeywordTypes.ON, BuildTooltip("Описує умову, за якою з'єднуються таблиці в JOIN.", "JOIN і назва приєднаної таблиці", "порівняння колонок двох таблиць", "ON users.id = orders.user_id") },
        { KeywordTypes.INNER, BuildTooltip("Уточнює JOIN так, щоб залишалися тільки рядки зі збігами в обох таблицях.", "FROM або інша JOIN-конструкція", "JOIN", "INNER JOIN orders ON users.id = orders.user_id") },
        { KeywordTypes.LEFT, BuildTooltip("Уточнює JOIN так, щоб зберегти всі рядки з лівої таблиці.", "FROM або інша JOIN-конструкція", "JOIN", "LEFT JOIN orders ON users.id = orders.user_id") },
        { KeywordTypes.RIGHT, BuildTooltip("Уточнює JOIN так, щоб зберегти всі рядки з правої таблиці.", "FROM або інша JOIN-конструкція", "JOIN", "RIGHT JOIN orders ON users.id = orders.user_id") },
        { KeywordTypes.FULL, BuildTooltip("Уточнює JOIN так, щоб зберегти рядки з обох таблиць навіть без збігів.", "FROM або інша JOIN-конструкція", "OUTER JOIN", "FULL OUTER JOIN orders ON users.id = orders.user_id") },
        { KeywordTypes.CROSS, BuildTooltip("Уточнює JOIN як декартовий добуток двох таблиць.", "FROM або інша JOIN-конструкція", "JOIN", "CROSS JOIN colors") },
        { KeywordTypes.OUTER, BuildTooltip("Підкреслює зовнішнє з'єднання для LEFT, RIGHT або FULL JOIN.", "LEFT, RIGHT або FULL", "JOIN", "LEFT OUTER JOIN orders ON users.id = orders.user_id") },
        { KeywordTypes.INNER_JOIN, BuildTooltip("Об'єднує тільки ті рядки, для яких умова ON має збіг в обох таблицях.", "FROM або інша JOIN-конструкція", "назва таблиці та ON", "INNER JOIN orders ON users.id = orders.user_id") },
        { KeywordTypes.LEFT_JOIN, BuildTooltip("Зберігає всі рядки з лівої таблиці та додає збіги з правої.", "FROM або інша JOIN-конструкція", "назва таблиці та ON", "LEFT JOIN orders ON users.id = orders.user_id") },
        { KeywordTypes.RIGHT_JOIN, BuildTooltip("Зберігає всі рядки з правої таблиці та додає збіги з лівої.", "FROM або інша JOIN-конструкція", "назва таблиці та ON", "RIGHT JOIN orders ON users.id = orders.user_id") },
        { KeywordTypes.FULL_JOIN, BuildTooltip("Зберігає рядки з обох таблиць, навіть якщо для частини з них немає збігу.", "FROM або інша JOIN-конструкція", "назва таблиці та ON", "FULL JOIN orders ON users.id = orders.user_id") },
        { KeywordTypes.CROSS_JOIN, BuildTooltip("Створює всі можливі комбінації рядків двох таблиць.", "FROM або інша JOIN-конструкція", "назва таблиці", "CROSS JOIN colors") },
        { KeywordTypes.GROUP, BuildTooltip("Починає конструкцію GROUP BY для об'єднання рядків у групи.", "WHERE або FROM", "BY", "GROUP BY city") },
        { KeywordTypes.BY, BuildTooltip("Уточнює поля для GROUP BY або ORDER BY.", "GROUP або ORDER", "назва колонки чи вираз", "ORDER BY created_at DESC") },
        { KeywordTypes.GROUP_BY, BuildTooltip("Групує рядки за однаковими значеннями колонок для агрегатних функцій.", "WHERE або FROM", "колонки групування, HAVING або ORDER BY", "GROUP BY city") },
        { KeywordTypes.HAVING, BuildTooltip("Фільтрує вже сформовані групи після GROUP BY.", "GROUP BY", "умова з агрегатною функцією", "HAVING COUNT(*) > 1") },
        { KeywordTypes.ORDER, BuildTooltip("Починає конструкцію ORDER BY для сортування результату.", "WHERE, GROUP BY або HAVING", "BY", "ORDER BY age DESC") },
        { KeywordTypes.ORDER_BY, BuildTooltip("Сортує результат запиту за колонкою або виразом.", "WHERE, GROUP BY або HAVING", "колонка, ASC, DESC, LIMIT або кінець запиту", "ORDER BY age DESC") },
        { KeywordTypes.ASC, BuildTooltip("Задає сортування за зростанням.", "назва колонки в ORDER BY", "наступна колонка сортування, LIMIT або кінець запиту", "ORDER BY age ASC") },
        { KeywordTypes.DESC, BuildTooltip("Задає сортування за спаданням.", "назва колонки в ORDER BY", "наступна колонка сортування, LIMIT або кінець запиту", "ORDER BY age DESC") },
        { KeywordTypes.LIMIT, BuildTooltip("Обмежує кількість рядків у результаті.", "ORDER BY, WHERE або FROM", "число рядків або OFFSET", "LIMIT 10") },
        { KeywordTypes.OFFSET, BuildTooltip("Пропускає вказану кількість перших рядків результату.", "LIMIT", "число рядків для пропуску", "LIMIT 10 OFFSET 20") },
        { KeywordTypes.INTO, BuildTooltip("Уточнює таблицю, до якої додаються дані в INSERT.", "INSERT", "назва таблиці та список колонок", "INSERT INTO users (name)") },
        { KeywordTypes.INSERT_INTO, BuildTooltip("Вказує таблицю, до якої потрібно додати нові значення.", "початок INSERT-запиту", "назва таблиці, колонки та VALUES", "INSERT INTO users (name) VALUES ('Anna')") },
        { KeywordTypes.VALUES, BuildTooltip("Передає конкретні значення для нового рядка в INSERT.", "INSERT INTO з таблицею і колонками", "список значень у дужках", "VALUES ('Anna', 18)") },
        { KeywordTypes.SET, BuildTooltip("Перелічує колонки та нові значення в UPDATE.", "UPDATE і назва таблиці", "присвоєння колонка = значення та WHERE", "SET name = 'Anna'") },
        { KeywordTypes.UPDATE_SET, BuildTooltip("Поєднує вибір таблиці для оновлення та перелік змін.", "початок UPDATE-запиту", "присвоєння та бажано WHERE", "UPDATE users SET age = 19 WHERE id = 1") },
        { KeywordTypes.TABLE, BuildTooltip("Позначає, що дія CREATE, ALTER або DROP виконується над таблицею.", "CREATE, ALTER або DROP", "назва таблиці", "CREATE TABLE users") },
        { KeywordTypes.CREATE_TABLE, BuildTooltip("Створює нову таблицю з указаними колонками та типами даних.", "початок запиту", "назва таблиці та опис колонок", "CREATE TABLE users (id INTEGER PRIMARY KEY)") },
        { KeywordTypes.ALTER_TABLE, BuildTooltip("Змінює структуру наявної таблиці.", "початок запиту", "назва таблиці та дія зміни", "ALTER TABLE users ADD COLUMN email TEXT") },
        { KeywordTypes.DROP_TABLE, BuildTooltip("Повністю видаляє таблицю разом із її даними.", "початок запиту", "назва таблиці", "DROP TABLE old_users") },
        { KeywordTypes.DELETE_FROM, BuildTooltip("Видаляє рядки з указанної таблиці.", "початок DELETE-запиту", "назва таблиці та бажано WHERE", "DELETE FROM users WHERE id = 1") },
        { KeywordTypes.AND, BuildTooltip("Поєднує дві умови, які мають бути істинними одночасно.", "перша умова у WHERE, ON або HAVING", "друга умова", "WHERE age >= 18 AND city = 'Kyiv'") },
        { KeywordTypes.OR, BuildTooltip("Поєднує дві умови, з яких достатньо виконати хоча б одну.", "перша умова у WHERE, ON або HAVING", "друга умова", "WHERE city = 'Kyiv' OR city = 'Lviv'") },
        { KeywordTypes.NOT, BuildTooltip("Заперечує умову або оператор.", "WHERE, AND або OR", "умова, IN, LIKE, BETWEEN або EXISTS", "WHERE NOT status = 'blocked'") },
        { KeywordTypes.IS, BuildTooltip("Перевіряє спеціальні значення, найчастіше NULL.", "назва колонки", "NULL або NOT NULL", "WHERE deleted_at IS NULL") },
        { KeywordTypes.IS_NOT, BuildTooltip("Перевіряє, що значення не дорівнює спеціальному значенню, найчастіше NULL.", "назва колонки", "NULL", "WHERE deleted_at IS NOT NULL") },
        { KeywordTypes.NULL, BuildTooltip("Позначає відсутність значення в колонці.", "IS, IS NOT або DEFAULT", "AND, OR або кінець умови", "WHERE phone IS NULL") },
        { KeywordTypes.IN, BuildTooltip("Перевіряє, чи належить значення до списку або результату підзапиту.", "назва колонки", "список у дужках або підзапит", "WHERE city IN ('Kyiv', 'Lviv')") },
        { KeywordTypes.LIKE, BuildTooltip("Порівнює текст із шаблоном, де % означає будь-яку послідовність символів.", "текстова колонка у WHERE", "рядок-шаблон", "WHERE name LIKE 'A%'") },
        { KeywordTypes.BETWEEN, BuildTooltip("Перевіряє, чи значення потрапляє в діапазон включно з межами.", "назва колонки", "нижня межа, AND і верхня межа", "WHERE age BETWEEN 18 AND 30") },
        { KeywordTypes.EXISTS, BuildTooltip("Перевіряє, чи підзапит повертає хоча б один рядок.", "WHERE або NOT", "підзапит у дужках", "WHERE EXISTS (SELECT 1 FROM orders)") },
        { KeywordTypes.UNION, BuildTooltip("Об'єднує результати двох SELECT-запитів і прибирає дублікати.", "перший SELECT-запит", "другий SELECT-запит", "SELECT city FROM users UNION SELECT city FROM offices") },
        { KeywordTypes.UNION_ALL, BuildTooltip("Об'єднує результати двох SELECT-запитів без вилучення дублікатів.", "перший SELECT-запит", "другий SELECT-запит", "SELECT city FROM users UNION ALL SELECT city FROM offices") },
        { KeywordTypes.ALL, BuildTooltip("Уточнює UNION так, щоб зберегти дублікати.", "UNION", "другий SELECT-запит", "UNION ALL SELECT city FROM offices") },
        { KeywordTypes.CASE, BuildTooltip("Починає умовний вираз, який повертає різні значення залежно від умов.", "SELECT, ORDER BY або SET", "WHEN", "CASE WHEN age >= 18 THEN 'adult' ELSE 'minor' END") },
        { KeywordTypes.WHEN, BuildTooltip("Задає одну умову всередині CASE.", "CASE або попередня гілка THEN", "умова та THEN", "WHEN score >= 90 THEN 'A'") },
        { KeywordTypes.THEN, BuildTooltip("Вказує результат для умови WHEN у CASE.", "умова WHEN", "значення, ELSE, WHEN або END", "THEN 'active'") },
        { KeywordTypes.ELSE, BuildTooltip("Задає результат CASE, якщо жодна умова WHEN не спрацювала.", "усі потрібні WHEN-гілки", "значення та END", "ELSE 'unknown'") },
        { KeywordTypes.END, BuildTooltip("Завершує конструкцію CASE.", "WHEN/THEN або ELSE", "AS, кома, FROM або кінець виразу", "END AS status_label") },
        { KeywordTypes.COUNT, BuildTooltip("Агрегатна функція, що рахує кількість рядків або непорожніх значень.", "SELECT або HAVING", "дужки з * або назвою колонки", "COUNT(*)") },
        { KeywordTypes.SUM, BuildTooltip("Агрегатна функція, що обчислює суму числових значень.", "SELECT або HAVING", "дужки з числовою колонкою", "SUM(amount)") },
        { KeywordTypes.AVG, BuildTooltip("Агрегатна функція, що обчислює середнє значення.", "SELECT або HAVING", "дужки з числовою колонкою", "AVG(score)") },
        { KeywordTypes.MIN, BuildTooltip("Агрегатна функція, що знаходить найменше значення.", "SELECT або HAVING", "дужки з назвою колонки", "MIN(price)") },
        { KeywordTypes.MAX, BuildTooltip("Агрегатна функція, що знаходить найбільше значення.", "SELECT або HAVING", "дужки з назвою колонки", "MAX(price)") },
        { KeywordTypes.PRIMARY, BuildTooltip("Починає опис первинного ключа, який унікально ідентифікує рядок.", "назва колонки або обмеження таблиці", "KEY", "id INTEGER PRIMARY KEY") },
        { KeywordTypes.KEY, BuildTooltip("Завершує конструкції PRIMARY KEY або FOREIGN KEY.", "PRIMARY або FOREIGN", "додаткові параметри чи REFERENCES", "PRIMARY KEY") },
        { KeywordTypes.PRIMARY_KEY, BuildTooltip("Визначає колонку або набір колонок як унікальний ідентифікатор рядка.", "тип колонки або CONSTRAINT", "AUTOINCREMENT, кома або кінець опису колонки", "id INTEGER PRIMARY KEY") },
        { KeywordTypes.FOREIGN, BuildTooltip("Починає опис зовнішнього ключа для зв'язку з іншою таблицею.", "опис колонки або CONSTRAINT", "KEY та REFERENCES", "FOREIGN KEY (user_id) REFERENCES users(id)") },
        { KeywordTypes.FOREIGN_KEY, BuildTooltip("Визначає зв'язок колонки з ключем іншої таблиці.", "опис таблиці або CONSTRAINT", "REFERENCES", "FOREIGN KEY (user_id) REFERENCES users(id)") },
        { KeywordTypes.REFERENCES, BuildTooltip("Вказує таблицю та колонку, на які посилається зовнішній ключ.", "FOREIGN KEY або колонка з REFERENCES", "назва таблиці та колонка в дужках", "REFERENCES users(id)") },
        { KeywordTypes.INTEGER, BuildTooltip("Тип даних для цілих чисел у SQLite.", "назва колонки в CREATE TABLE", "обмеження колонки або кома", "age INTEGER") },
        { KeywordTypes.TEXT, BuildTooltip("Тип даних для текстових значень у SQLite.", "назва колонки в CREATE TABLE", "обмеження колонки або кома", "name TEXT") },
        { KeywordTypes.REAL, BuildTooltip("Тип даних для чисел із дробовою частиною у SQLite.", "назва колонки в CREATE TABLE", "обмеження колонки або кома", "price REAL") },
        { KeywordTypes.BLOB, BuildTooltip("Тип даних для двійкових даних у SQLite.", "назва колонки в CREATE TABLE", "обмеження колонки або кома", "photo BLOB") },
        { KeywordTypes.UNIQUE, BuildTooltip("Забороняє дублікати в колонці або наборі колонок.", "тип колонки або CONSTRAINT", "кома або кінець опису", "email TEXT UNIQUE") },
        { KeywordTypes.DEFAULT, BuildTooltip("Задає значення, яке буде використане, якщо його не передали явно.", "тип колонки або інші обмеження", "значення за замовчуванням", "created_at TEXT DEFAULT CURRENT_TIMESTAMP") },
        { KeywordTypes.CHECK, BuildTooltip("Додає правило перевірки значення під час вставки або оновлення.", "опис колонки або CONSTRAINT", "умова в дужках", "CHECK (age >= 0)") },
        { KeywordTypes.AUTOINCREMENT, BuildTooltip("У SQLite змушує INTEGER PRIMARY KEY генерувати нові значення автоматично.", "INTEGER PRIMARY KEY", "кома або кінець опису колонки", "id INTEGER PRIMARY KEY AUTOINCREMENT") }
    };

    private static readonly Dictionary<string, KeywordTypes> KeywordAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        { "INNER JOIN", KeywordTypes.INNER_JOIN },
        { "LEFT JOIN", KeywordTypes.LEFT_JOIN },
        { "LEFT OUTER JOIN", KeywordTypes.LEFT_JOIN },
        { "RIGHT JOIN", KeywordTypes.RIGHT_JOIN },
        { "RIGHT OUTER JOIN", KeywordTypes.RIGHT_JOIN },
        { "FULL JOIN", KeywordTypes.FULL_JOIN },
        { "FULL OUTER JOIN", KeywordTypes.FULL_JOIN },
        { "CROSS JOIN", KeywordTypes.CROSS_JOIN },
        { "GROUP BY", KeywordTypes.GROUP_BY },
        { "ORDER BY", KeywordTypes.ORDER_BY },
        { "INSERT INTO", KeywordTypes.INSERT_INTO },
        { "UPDATE SET", KeywordTypes.UPDATE_SET },
        { "CREATE TABLE", KeywordTypes.CREATE_TABLE },
        { "ALTER TABLE", KeywordTypes.ALTER_TABLE },
        { "DROP TABLE", KeywordTypes.DROP_TABLE },
        { "DELETE FROM", KeywordTypes.DELETE_FROM },
        { "IS NOT", KeywordTypes.IS_NOT },
        { "UNION ALL", KeywordTypes.UNION_ALL },
        { "PRIMARY KEY", KeywordTypes.PRIMARY_KEY },
        { "FOREIGN KEY", KeywordTypes.FOREIGN_KEY }
    };

    private static readonly string[] PrefixTooltipKeys =
    {
        "LEFT OUTER JOIN",
        "RIGHT OUTER JOIN",
        "FULL OUTER JOIN",
        "INNER JOIN",
        "LEFT JOIN",
        "RIGHT JOIN",
        "FULL JOIN",
        "CROSS JOIN",
        "CREATE TABLE",
        "ALTER TABLE",
        "DROP TABLE",
        "DELETE FROM",
        "INSERT INTO",
        "UPDATE",
        "SELECT",
        "WHERE",
        "GROUP BY",
        "HAVING",
        "ORDER BY",
        "LIMIT",
        "OFFSET",
        "VALUES",
        "SET",
        "JOIN",
        "ON"
    };

    private static readonly Dictionary<string, string> OperatorTooltips = new()
    {
        { "=", BuildTooltip("Перевіряє рівність двох значень або присвоює значення в SET.", "колонка або вираз", "значення, колонка або вираз", "WHERE id = 1") },
        { "!=", BuildTooltip("Перевіряє, що два значення не рівні.", "колонка або вираз", "значення, колонка або вираз", "WHERE status != 'closed'") },
        { "<>", BuildTooltip("Стандартний SQL-оператор нерівності.", "колонка або вираз", "значення, колонка або вираз", "WHERE status <> 'closed'") },
        { ">", BuildTooltip("Перевіряє, що ліве значення більше за праве.", "числова колонка або вираз", "число, колонка або вираз", "WHERE age > 18") },
        { ">=", BuildTooltip("Перевіряє, що ліве значення більше або дорівнює правому.", "числова колонка або вираз", "число, колонка або вираз", "WHERE age >= 18") },
        { "<", BuildTooltip("Перевіряє, що ліве значення менше за праве.", "числова колонка або вираз", "число, колонка або вираз", "WHERE price < 100") },
        { "<=", BuildTooltip("Перевіряє, що ліве значення менше або дорівнює правому.", "числова колонка або вираз", "число, колонка або вираз", "WHERE price <= 100") },
        { ",", "Розділяє колонки, значення або умови в списку.\nЗазвичай після: елемент списку.\nЗазвичай перед: наступний елемент списку.\nПриклад: SELECT name, age" },
        { "*", "Позначає всі колонки таблиці у SELECT або всі рядки в COUNT(*).\nЗазвичай після: SELECT або COUNT(.\nЗазвичай перед: FROM або ).\nПриклад: SELECT * FROM users" },
        { "(", "Відкриває список значень, колонок, аргументів функції або підзапит.\nЗазвичай після: функція, IN, VALUES або назва таблиці в CREATE TABLE.\nЗазвичай перед: перший елемент списку.\nПриклад: VALUES ('Anna', 18)" },
        { ")", "Закриває список значень, колонок, аргументів функції або підзапит.\nЗазвичай після: останній елемент списку.\nЗазвичай перед: кома, AND, OR або кінець фрагмента.\nПриклад: COUNT(*)" },
        { ";", "Позначає завершення SQL-команди.\nЗазвичай після: остання частина запиту.\nЗазвичай перед: новий запит або нічого.\nПриклад: SELECT * FROM users;" }
    };

    public static string GetTooltip(BlockType blockType, KeywordTypes keywordType, string blockValue)
    {
        if (blockType == BlockType.Keyword &&
            keywordType != KeywordTypes.none &&
            Tooltips.TryGetValue(keywordType, out string keywordTooltip))
        {
            return keywordTooltip;
        }

        if (CanUseSqlTextTooltip(blockType) &&
            TryGetTooltipByText(blockValue, out string textTooltip))
        {
            return textTooltip;
        }

        return blockType switch
        {
            BlockType.Table => BuildTableTooltip(blockValue),
            BlockType.Column => BuildColumnTooltip(blockValue),
            BlockType.Operator => BuildOperatorTooltip(blockValue),
            BlockType.Value => BuildValueTooltip(blockValue),
            BlockType.Statement => BuildStatementTooltip(blockValue),
            _ => $"{blockType}: {blockValue}"
        };
    }

    public static string GetDisplayText(KeywordTypes keywordType, string blockValue)
    {
        if (keywordType == KeywordTypes.none)
            return blockValue;

        return keywordType.ToString().Replace('_', ' ');
    }

    public static bool TryParseKeywordType(string value, out KeywordTypes keywordType)
    {
        keywordType = KeywordTypes.none;

        string normalizedText = NormalizeSqlText(value);
        if (string.IsNullOrEmpty(normalizedText))
            return false;

        if (KeywordAliases.TryGetValue(normalizedText, out keywordType))
            return true;

        string enumName = NormalizeKeywordTypeName(normalizedText);
        if (Enum.TryParse(enumName, true, out keywordType) &&
            keywordType != KeywordTypes.none)
        {
            return true;
        }

        string functionName = ExtractFunctionName(normalizedText);
        if (!string.IsNullOrEmpty(functionName) &&
            Enum.TryParse(functionName, true, out keywordType) &&
            Tooltips.ContainsKey(keywordType))
        {
            return true;
        }

        keywordType = KeywordTypes.none;
        return false;
    }

    private static bool TryGetTooltipByText(string value, out string tooltip)
    {
        tooltip = null;

        if (TryParseKeywordType(value, out KeywordTypes keywordType) &&
            Tooltips.TryGetValue(keywordType, out tooltip))
        {
            return true;
        }

        string normalizedText = NormalizeSqlText(value);
        if (string.IsNullOrEmpty(normalizedText))
            return false;

        foreach (string prefix in PrefixTooltipKeys)
        {
            if (StartsWithSqlToken(normalizedText, prefix) &&
                TryParseKeywordType(prefix, out KeywordTypes prefixType) &&
                Tooltips.TryGetValue(prefixType, out tooltip))
            {
                return true;
            }
        }

        return false;
    }

    private static bool CanUseSqlTextTooltip(BlockType blockType)
    {
        return blockType is BlockType.Keyword or BlockType.Operator or BlockType.Value or BlockType.Statement;
    }

    private static string BuildTooltip(string explanation, string usuallyAfter, string usuallyBefore, string example)
    {
        return $"{explanation}\nЗазвичай після: {usuallyAfter}.\nЗазвичай перед: {usuallyBefore}.\nПриклад: {example}";
    }

    private static string BuildTableTooltip(string blockValue)
    {
        return $"Таблиця бази даних.\nНазва: {blockValue}\nЗазвичай після: FROM, JOIN, UPDATE, INTO, TABLE або REFERENCES.\nЗазвичай перед: WHERE, JOIN, ON, SET, список колонок або кінець запиту.\nПриклад: FROM {SafeExampleValue(blockValue, "users")}";
    }

    private static string BuildColumnTooltip(string blockValue)
    {
        return $"Колонка таблиці.\nНазва: {blockValue}\nЗазвичай після: SELECT, WHERE, ON, GROUP BY, ORDER BY або назви таблиці.\nЗазвичай перед: оператор порівняння, AS, кома, FROM або тип даних.\nПриклад: SELECT {SafeExampleValue(blockValue, "name")} FROM users";
    }

    private static string BuildOperatorTooltip(string blockValue)
    {
        string normalizedOperator = NormalizeOperator(blockValue);

        if (OperatorTooltips.TryGetValue(normalizedOperator, out string tooltip))
            return tooltip;

        return $"Оператор умови, порівняння або розділення.\nЗначення: {blockValue}\nЗазвичай після: колонка, значення або вираз.\nЗазвичай перед: значення, колонка або наступна частина виразу.\nПриклад: WHERE age {SafeExampleValue(blockValue, "=")} 18";
    }

    private static string BuildValueTooltip(string blockValue)
    {
        return $"Значення, яке використовується у запиті.\nЗначення: {blockValue}\nЗазвичай після: оператор порівняння, VALUES, LIMIT, OFFSET або DEFAULT.\nЗазвичай перед: AND, OR, кома, дужка або кінець умови.\nПриклад: WHERE name = {FormatValueExample(blockValue)}";
    }

    private static string BuildStatementTooltip(string blockValue)
    {
        return $"SQL-фрагмент або готова частина запиту.\nФрагмент: {blockValue}\nЗазвичай після: попередня логічна частина запиту або початок команди.\nЗазвичай перед: наступна частина запиту або завершення команди.\nПриклад: {SafeExampleValue(blockValue, "SELECT * FROM users")}";
    }

    private static string NormalizeSqlText(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        string trimmed = value.Trim().Trim(';', ',');
        string[] parts = trimmed.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);

        return string.Join(" ", parts).ToUpperInvariant();
    }

    private static string NormalizeKeywordTypeName(string value)
    {
        return NormalizeSqlText(value)
            .Replace(" ", "_")
            .Replace("(", string.Empty)
            .Replace(")", string.Empty)
            .Replace("*", string.Empty);
    }

    private static string ExtractFunctionName(string normalizedText)
    {
        int parenthesisIndex = normalizedText.IndexOf('(');
        if (parenthesisIndex <= 0)
            return string.Empty;

        return NormalizeKeywordTypeName(normalizedText[..parenthesisIndex]);
    }

    private static bool StartsWithSqlToken(string normalizedText, string prefix)
    {
        if (!normalizedText.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            return false;

        return normalizedText.Length == prefix.Length ||
               char.IsWhiteSpace(normalizedText[prefix.Length]) ||
               normalizedText[prefix.Length] == '(';
    }

    private static string NormalizeOperator(string value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : value.Trim().ToUpperInvariant();
    }

    private static string SafeExampleValue(string value, string fallback)
    {
        return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
    }

    private static string FormatValueExample(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "'Anna'";

        string trimmed = value.Trim();

        if (trimmed.StartsWith("'") || trimmed.StartsWith("\"") || decimal.TryParse(trimmed, out _))
            return trimmed;

        return $"'{trimmed}'";
    }
}
