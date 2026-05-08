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
		{ KeywordTypes.SELECT, "Вибирає колонки з таблиці.\nПриклад: SELECT * " },
		{ KeywordTypes.FROM, "Вказує джерело даних.\nПриклад: FROM users" },
		{ KeywordTypes.WHERE, "Умова фільтрації.\nПриклад: WHERE id = 1" },
		{ KeywordTypes.JOIN, "Об'єднує таблиці за ключем." }
	};
}