using FlowMapper.Abstractions;

namespace FlowMapper.Providers.PostgreSql;

public class PostgreSqlDialect : IDialect
{
    private string? _ftsLanguage;

    public string? FtsLanguage
    {
        get => _ftsLanguage;
        set => _ftsLanguage = value;
    }

    public PostgreSqlDialect()
    {
    }

    public PostgreSqlDialect(string? ftsLanguage)
    {
        _ftsLanguage = ftsLanguage;
    }

    public string ApplyPagination(string sql, int offset, int limit)
    {
        return $"{sql} LIMIT {limit} OFFSET {offset}";
    }

    public string GetIdentityQuery() => "SELECT LASTVAL()";

    public string NormalizeParameter(string name) => $"@{name}";

    public string BuildFreeTextCondition(IReadOnlyList<string> columns, string parameterName)
    {
        var lang = _ftsLanguage ?? "english";
        var concat = string.Join(" || ' ' || ", columns);
        return $"to_tsvector('{lang}', {concat}) @@ plainto_tsquery('{lang}', {parameterName})";
    }

    public string BuildContainsCondition(IReadOnlyList<string> columns, string parameterName)
    {
        var lang = _ftsLanguage ?? "english";
        var concat = string.Join(" || ' ' || ", columns);
        return $"to_tsvector('{lang}', {concat}) @@ to_tsquery('{lang}', {parameterName})";
    }

    public string BuildRankOrderBy(IReadOnlyList<string> columns, string parameterName)
    {
        var lang = _ftsLanguage ?? "english";
        var concat = string.Join(" || ' ' || ", columns);
        return $"ts_rank(to_tsvector('{lang}', {concat}), plainto_tsquery('{lang}', {parameterName})) DESC";
    }

    public string? VerifyFtsIndexSql(string table, string column)
    {
        return $"SELECT 1 FROM pg_indexes WHERE tablename = '{table}' AND indexdef LIKE '%to_tsvector%'";
    }

    public string? FtsIndexErrorMessage => null;

    public bool FtsRequiresIndex => false;
    public bool FtsSupportsLanguage => true;
}
