using System.Data;
using FlowMapper.Abstractions;
using Npgsql;

namespace FlowMapper.Providers.PostgreSql;

public class PostgreSqlProvider : IDatabaseProvider
{
    private readonly string _connectionString;

    public string Name => "PostgreSQL";

    public IDialect Dialect => new PostgreSqlDialect();

    public Version Version => new(2, 0);

    public PostgreSqlProvider(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IDbConnection CreateConnection() =>
        new NpgsqlConnection(_connectionString);

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
        var param = new NpgsqlParameter(name, value ?? DBNull.Value);
        return param;
    }
}
