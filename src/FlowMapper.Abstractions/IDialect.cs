namespace FlowMapper.Abstractions;

public interface IDialect
{
    string ApplyPagination(string sql, int offset, int limit);
    string GetIdentityQuery();
    string NormalizeParameter(string name);
}
