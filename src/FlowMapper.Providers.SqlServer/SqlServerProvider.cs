using System.Data;
using FlowMapper.Abstractions;
using Microsoft.Data.SqlClient;

namespace FlowMapper.Providers.SqlServer;

public class SqlServerProvider : IDatabaseProvider
{
    private readonly string _connectionString;
    private readonly SqlServerDialect _dialect;

    public string Name => "SqlServer";

    public IDialect Dialect => _dialect;

    public Version Version => new(2, 0);

    public SqlServerProvider(string connectionString)
        : this(connectionString, null)
    {
    }

    public SqlServerProvider(string connectionString, string? ftsLanguage)
    {
        _connectionString = connectionString;
        _dialect = new SqlServerDialect(ftsLanguage);
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
