using FlowMapper.Abstractions;

namespace FlowMapper.Providers.Oracle;

public class OracleDialect : IDialect
{
    private string? _ftsLanguage;

    public string? FtsLanguage
    {
        get => _ftsLanguage;
        set => _ftsLanguage = value;
    }

    public OracleDialect()
    {
    }

    public OracleDialect(string? ftsLanguage)
    {
        _ftsLanguage = ftsLanguage;
    }

    public string ApplyPagination(string sql, int offset, int limit)
    {
        return $"SELECT * FROM (SELECT a.*, ROWNUM rnum FROM ({sql}) a WHERE ROWNUM <= {offset + limit}) WHERE rnum > {offset}";
    }

    public string GetIdentityQuery() => "SELECT LAST_INSERT_ID()";

    public string NormalizeParameter(string name) => $":{name}";

    public string BuildFreeTextCondition(IReadOnlyList<string> columns, string parameterName)
    {
        var cols = string.Join(", ", columns);
        return $"CONTAINS({cols}, {parameterName}, 1) > 0";
    }

    public string BuildContainsCondition(IReadOnlyList<string> columns, string parameterName)
    {
        var cols = string.Join(", ", columns);
        return $"CONTAINS({cols}, {parameterName}) > 0";
    }

    public string BuildRankOrderBy(IReadOnlyList<string> columns, string parameterName)
    {
        return "SCORE(1) DESC";
    }

    public string? VerifyFtsIndexSql(string table, string column)
    {
        return $"SELECT 1 FROM ctx_user_indexes WHERE ...";
    }

    public string? FtsIndexErrorMessage =>
        "FTS requires an Oracle Text index. " +
        "Run: CREATE INDEX [idx] ON [table]([column]) INDEXTYPE IS CTXSYS.CONTEXT;";

    public bool FtsRequiresIndex => true;
    public bool FtsSupportsLanguage => false;
}
