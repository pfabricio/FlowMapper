using FlowMapper.Abstractions;

namespace FlowMapper.Providers.MySql;

public class MySqlDialect : IDialect
{
    private string? _ftsLanguage;

    public string? FtsLanguage
    {
        get => _ftsLanguage;
        set => _ftsLanguage = value;
    }

    public MySqlDialect()
    {
    }

    public MySqlDialect(string? ftsLanguage)
    {
        _ftsLanguage = ftsLanguage;
    }

    public string ApplyPagination(string sql, int offset, int limit)
    {
        return $"{sql} LIMIT {limit} OFFSET {offset}";
    }

    public string GetIdentityQuery() => "SELECT LAST_INSERT_ID()";

    public string NormalizeParameter(string name) => $"@{name}";

    public string BuildFreeTextCondition(IReadOnlyList<string> columns, string parameterName)
    {
        var cols = string.Join(", ", columns);
        return $"MATCH({cols}) AGAINST ({parameterName} IN NATURAL LANGUAGE MODE)";
    }

    public string BuildContainsCondition(IReadOnlyList<string> columns, string parameterName)
    {
        var cols = string.Join(", ", columns);
        return $"MATCH({cols}) AGAINST ({parameterName} IN BOOLEAN MODE)";
    }

    public string BuildRankOrderBy(IReadOnlyList<string> columns, string parameterName)
    {
        var cols = string.Join(", ", columns);
        return $"MATCH({cols}) AGAINST ({parameterName}) DESC";
    }

    public string? VerifyFtsIndexSql(string table, string column)
    {
        return $"SELECT 1 FROM INFORMATION_SCHEMA.STATISTICS " +
               $"WHERE TABLE_NAME = '{table}' AND INDEX_TYPE = 'FULLTEXT' AND COLUMN_NAME = '{column}'";
    }

    public string? FtsIndexErrorMessage => null;

    public bool FtsRequiresIndex => false;
    public bool FtsSupportsLanguage => false;
}
