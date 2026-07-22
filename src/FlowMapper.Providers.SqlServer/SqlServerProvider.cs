using System.Data;
using FlowMapper.Abstractions;
using Microsoft.Data.SqlClient;

namespace FlowMapper.Providers.SqlServer;

public class SqlServerProvider : IDatabaseProvider
{
    private readonly string _connectionString;

    public string Name => "SqlServer";

    public IDialect Dialect => new SqlServerDialect();

    public Version Version => new(2, 0);

    public SqlServerProvider(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IDbConnection CreateConnection() =>
        new SqlConnection(_connectionString);

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
        var param = new SqlParameter(name, value ?? DBNull.Value);
        return param;
    }
}
