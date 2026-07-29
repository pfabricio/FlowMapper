namespace FlowMapper.Abstractions;

public interface IDialect
{
    string ApplyPagination(string sql, int offset, int limit);
    string GetIdentityQuery();
    string NormalizeParameter(string name);

    string BuildFreeTextCondition(IReadOnlyList<string> columns, string parameterName);
    string BuildContainsCondition(IReadOnlyList<string> columns, string parameterName);
    string BuildRankOrderBy(IReadOnlyList<string> columns, string parameterName);
    string? VerifyFtsIndexSql(string table, string column);
    string? FtsIndexErrorMessage { get; }
    bool FtsRequiresIndex { get; }
    bool FtsSupportsLanguage { get; }
}
