using System.Data;
using FlowMapper.Abstractions;
using Npgsql;

namespace FlowMapper.Providers.PostgreSql;

public class PostgreSqlProvider : IDatabaseProvider
{
    private readonly string _connectionString;
    private readonly PostgreSqlDialect _dialect;

    public string Name => "PostgreSQL";

    public IDialect Dialect => _dialect;

    public Version Version => new(2, 0);

    public PostgreSqlProvider(string connectionString)
        : this(connectionString, null)
    {
    }

    public PostgreSqlProvider(string connectionString, string? ftsLanguage)
    {
        _connectionString = connectionString;
        _dialect = new PostgreSqlDialect(ftsLanguage);
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
