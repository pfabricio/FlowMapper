using FlowMapper.Abstractions;

namespace FlowMapper.Providers.SqlServer;

public class SqlServerDialect : IDialect
{
    public string ApplyPagination(string sql, int offset, int limit)
    {
        return $"{sql} OFFSET {offset} ROWS FETCH NEXT {limit} ROWS ONLY";
    }

    public string GetIdentityQuery() => "SELECT SCOPE_IDENTITY()";

    public string NormalizeParameter(string name) => $"@{name}";
}
