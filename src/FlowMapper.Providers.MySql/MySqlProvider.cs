using System.Data;
using FlowMapper.Abstractions;
using MySqlConnector;

namespace FlowMapper.Providers.MySql;

public class MySqlProvider : IDatabaseProvider
{
    private readonly string _connectionString;

    public string Name => "MySQL";

    public IDialect Dialect => new MySqlDialect();

    public Version Version => new(2, 0);

    public MySqlProvider(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IDbConnection CreateConnection() =>
        new MySqlConnection(_connectionString);

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
        var param = new MySqlParameter(name, value ?? DBNull.Value);
        return param;
    }
}
