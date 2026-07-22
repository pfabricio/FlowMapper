using System.Data;
using System.Data.Common;
using FlowMapper.Abstractions;
using FlowMapper.Providers.Abstractions;

namespace FlowMapper.Runtime;

public class CommandExecutor : ICommandExecutor
{
    private readonly IConnectionFactory _connectionFactory;

    public CommandExecutor(IConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<int> ExecuteAsync(string sql, object? parameters = null, ExecutionOptions? options = null, CancellationToken ct = default)
    {
        var connection = _connectionFactory.CreateConnection();
        connection.Open();
        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.CommandType = options?.CommandType ?? CommandType.Text;
            if (options?.Timeout.HasValue == true)
                command.CommandTimeout = options.Timeout.Value;
            ApplyParameters(command, parameters);

            if (command is DbCommand dbCmd)
                return await dbCmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);

            return command.ExecuteNonQuery();
        }
        finally
        {
            connection.Close();
            connection.Dispose();
        }
    }

    public async Task<T> ExecuteScalarAsync<T>(string sql, object? parameters = null, ExecutionOptions? options = null, CancellationToken ct = default)
    {
        var connection = _connectionFactory.CreateConnection();
        connection.Open();
        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.CommandType = options?.CommandType ?? CommandType.Text;
            if (options?.Timeout.HasValue == true)
                command.CommandTimeout = options.Timeout.Value;
            ApplyParameters(command, parameters);

            if (command is DbCommand dbCmd)
            {
                var result = await dbCmd.ExecuteScalarAsync(ct).ConfigureAwait(false);
                return (T)Convert.ChangeType(result, typeof(T))!;
            }

            return (T)Convert.ChangeType(command.ExecuteScalar(), typeof(T))!;
        }
        finally
        {
            connection.Close();
            connection.Dispose();
        }
    }

    private static void ApplyParameters(IDbCommand command, object? parameters)
    {
        if (parameters == null) return;

        foreach (var prop in parameters.GetType().GetProperties())
        {
            var param = command.CreateParameter();
            param.ParameterName = $"@{prop.Name}";
            param.Value = prop.GetValue(parameters) ?? DBNull.Value;
            command.Parameters.Add(param);
        }
    }
}
