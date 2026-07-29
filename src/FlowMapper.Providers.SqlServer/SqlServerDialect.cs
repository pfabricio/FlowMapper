using FlowMapper.Abstractions;

namespace FlowMapper.Providers.SqlServer;

public class SqlServerDialect : IDialect
{
    private string? _ftsLanguage;

    public string? FtsLanguage
    {
        get => _ftsLanguage;
        set => _ftsLanguage = value;
    }

    public SqlServerDialect()
    {
    }

    public SqlServerDialect(string? ftsLanguage)
    {
        _ftsLanguage = ftsLanguage;
    }

    public string ApplyPagination(string sql, int offset, int limit)
    {
        return $"{sql} OFFSET {offset} ROWS FETCH NEXT {limit} ROWS ONLY";
    }

    public string GetIdentityQuery() => "SELECT SCOPE_IDENTITY()";

    public string NormalizeParameter(string name) => $"@{name}";

    public string BuildFreeTextCondition(IReadOnlyList<string> columns, string parameterName)
    {
        var cols = string.Join(", ", columns);
        return $"FREETEXT(({cols}), {parameterName})";
    }

    public string BuildContainsCondition(IReadOnlyList<string> columns, string parameterName)
    {
        var cols = string.Join(", ", columns);
        return $"CONTAINS(({cols}), {parameterName})";
    }

    public string BuildRankOrderBy(IReadOnlyList<string> columns, string parameterName)
    {
        var cols = string.Join(", ", columns);
        return $"FREETEXTTABLE([dummy], ({cols}), {parameterName}, 1) AS _ft ORDER BY _ft.RANK DESC";
    }

    public string? VerifyFtsIndexSql(string table, string column)
    {
        return $"SELECT 1 FROM sys.fulltext_index_columns fic " +
               $"JOIN sys.columns c ON fic.column_id = c.column_id AND fic.object_id = c.object_id " +
               $"WHERE OBJECT_NAME(fic.object_id) = '{table}' AND c.name = '{column}'";
    }

    public string? FtsIndexErrorMessage =>
        "FTS requires a Full-Text index on SQL Server. " +
        "Run: CREATE FULLTEXT CATALOG ft_catalog AS DEFAULT; " +
        "CREATE FULLTEXT INDEX ON [table]([column]) KEY INDEX [pk] ON ft_catalog;";

    public bool FtsRequiresIndex => true;
    public bool FtsSupportsLanguage => false;
}
