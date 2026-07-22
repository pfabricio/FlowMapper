using FlowMapper.Abstractions;

namespace FlowMapper.Providers.MySql;

public class MySqlDialect : IDialect
{
    public string ApplyPagination(string sql, int offset, int limit)
    {
        return $"{sql} LIMIT {limit} OFFSET {offset}";
    }

    public string GetIdentityQuery() => "SELECT LAST_INSERT_ID()";

    public string NormalizeParameter(string name) => $"@{name}";
}
