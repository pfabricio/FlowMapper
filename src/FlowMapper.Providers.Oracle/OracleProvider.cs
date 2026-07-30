using System.Data;
using FlowMapper.Abstractions;
using Oracle.ManagedDataAccess.Client;

namespace FlowMapper.Providers.Oracle;

public class OracleProvider : IDatabaseProvider
{
    private readonly string _connectionString;
    private readonly OracleDialect _dialect;

    public string Name => "Oracle";

    public IDialect Dialect => _dialect;

    public Version Version => new(2, 0);

    public OracleProvider(string connectionString)
        : this(connectionString, null)
    {
    }

    public OracleProvider(string connectionString, string? ftsLanguage)
    {
        _connectionString = connectionString;
        _dialect = new OracleDialect(ftsLanguage);
    }

    public IDbConnection CreateConnection() =>
        new OracleConnection(_connectionString);

    public IDbCommand CreateCommand(string sql, IDbConnection connection, IDbTransaction? transaction = null)
    {
        var cmd = connection.CreateCommand();
        cmd.CommandText = sql;
        cmd.CommandType = CommandType.Text;
        if (transaction != null)
            cmd.Transaction = transaction;
        return cmd;
    }

    public IDataParameter CreateParameter(string name, object? value)
    {
        var param = new OracleParameter(name, value ?? DBNull.Value);
        return param;
    }
}
