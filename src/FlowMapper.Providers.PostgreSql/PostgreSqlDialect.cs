using FlowMapper.Abstractions;

namespace FlowMapper.Providers.PostgreSql;

public class PostgreSqlDialect : IDialect
{
    public string ApplyPagination(string sql, int offset, int limit)
    {
        return $"{sql} LIMIT {limit} OFFSET {offset}";
    }

    public string GetIdentityQuery() => "SELECT LASTVAL()";

    public string NormalizeParameter(string name) => $"@{name}";
}
