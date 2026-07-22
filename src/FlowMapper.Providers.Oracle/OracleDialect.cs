using FlowMapper.Abstractions;

namespace FlowMapper.Providers.Oracle;

public class OracleDialect : IDialect
{
    public string ApplyPagination(string sql, int offset, int limit)
    {
        return $"SELECT * FROM (SELECT a.*, ROWNUM rnum FROM ({sql}) a WHERE ROWNUM <= {offset + limit}) WHERE rnum > {offset}";
    }

    public string GetIdentityQuery() => "SELECT LAST_INSERT_ID()";

    public string NormalizeParameter(string name) => $":{name}";
}
