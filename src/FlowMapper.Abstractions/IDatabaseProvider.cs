using System.Data;

namespace FlowMapper.Abstractions;

public interface IDatabaseProvider
{
    string Name { get; }
    IDialect Dialect { get; }
    Version Version { get; }
    IDbConnection CreateConnection();
    IDbCommand CreateCommand(string sql, IDbConnection connection, IDbTransaction? transaction = null);
    IDataParameter CreateParameter(string name, object? value);
}
