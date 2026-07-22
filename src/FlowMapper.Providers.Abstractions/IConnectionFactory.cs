using System.Data;

namespace FlowMapper.Providers.Abstractions;

public interface IConnectionFactory
{
    IDbConnection CreateConnection(string? name = null);
}
