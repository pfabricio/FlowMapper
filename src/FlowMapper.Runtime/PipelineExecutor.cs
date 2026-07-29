using System.Data;
using System.Data.Common;
using System.Runtime.CompilerServices;
using FlowMapper.Abstractions;
using FlowMapper.Execution;
using FlowMapper.Materializer;
using FlowMapper.Providers.Abstractions;

namespace FlowMapper.Runtime;

public class PipelineExecutor : IPipelineExecutor
{
    private readonly IReadOnlyList<IPipelineBehavior> _behaviors;
    private readonly IConnectionFactory _connectionFactory;
    private readonly IMaterializer _materializer;
    private readonly IExecutionScopeFactory _scopeFactory;

    public PipelineExecutor(
        IEnumerable<IPipelineBehavior> behaviors,
        IConnectionFactory connectionFactory,
        IMaterializer materializer,
        IExecutionScopeFactory scopeFactory)
    {
        _behaviors = behaviors.OrderBy(b => b is IOrderedBehavior ob ? ob.Order : 1000).ToList();
        _connectionFactory = connectionFactory;
        _materializer = materializer;
        _scopeFactory = scopeFactory;
    }

    public Task<IEnumerable<T>> QueryAsync<T>(
        string sql, object? parameters = null,
        ExecutionOptions? options = null, CancellationToken ct = default)
    {
        var context = new ExecutionContext<IEnumerable<T>>(sql, parameters, options);
        return RunWithBehaviors(context, ct, () => ExecuteQueryAsync<T>(context, ct));
    }

    public Task<int> ExecuteAsync(
        string sql, object? parameters = null,
        ExecutionOptions? options = null, CancellationToken ct = default)
    {
        var context = new ExecutionContext<int>(sql, parameters, options);
        return RunWithBehaviors(context, ct, () => ExecuteNonQueryAsync(context, ct));
    }

    public async IAsyncEnumerable<T> StreamAsync<T>(
        string sql, object? parameters = null,
        ExecutionOptions? options = null, [EnumeratorCancellation] CancellationToken ct = default)
    {
        var connection = _connectionFactory.CreateConnection();
        connection.Open();

        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            ApplyParameters(command, parameters);

            using var reader = await ExecuteReaderAsync(command, ct).ConfigureAwait(false);
            var plan = _materializer.BuildPlan<T>();

            if (reader is DbDataReader dbReader)
            {
                while (await dbReader.ReadAsync(ct).ConfigureAwait(false))
                    yield return _materializer.Materialize<T>(reader, plan);
            }
            else
            {
                while (reader.Read())
                    yield return _materializer.Materialize<T>(reader, plan);
            }
        }
        finally
        {
            connection.Close();
            connection.Dispose();
        }
    }

    private async Task<TResult> RunWithBehaviors<TResult>(
        ExecutionContext<TResult> context, CancellationToken ct, Func<Task> coreAction)
    {
        var index = 0;

        async Task ExecuteChain()
        {
            if (index >= _behaviors.Count)
            {
                await coreAction();
                return;
            }

            var behavior = _behaviors[index++];

            if (!behavior.ShouldExecute(context))
            {
                await ExecuteChain();
                return;
            }

            await behavior.HandleAsync(context, ExecuteChain);
        }

        context.Phase = ExecutionPhase.BeforeExecute;

        await ExecuteChain();

        context.Phase = ExecutionPhase.Completed;
        return context.Result!;
    }

    private async Task ExecuteQueryAsync<T>(ExecutionContext<IEnumerable<T>> context, CancellationToken ct)
    {
        var sql = context.Sql;
        var parameters = context.Parameters;
        var options = context.Options;

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

            using var reader = await ExecuteReaderAsync(command, ct).ConfigureAwait(false);
            var plan = _materializer.BuildPlan<T>();
            var results = new List<T>();

            if (reader is DbDataReader dbReader)
            {
                while (await dbReader.ReadAsync(ct).ConfigureAwait(false))
                    results.Add(_materializer.Materialize<T>(reader, plan));
            }
            else
            {
                while (reader.Read())
                    results.Add(_materializer.Materialize<T>(reader, plan));
            }

            context.Result = results.AsEnumerable();
        }
        finally
        {
            connection.Close();
            connection.Dispose();
        }
    }

    private async Task ExecuteNonQueryAsync(ExecutionContext<int> context, CancellationToken ct)
    {
        var sql = context.Sql;
        var parameters = context.Parameters;
        var options = context.Options;

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

            int result;
            if (command is DbCommand dbCmd)
                result = await dbCmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            else
                result = command.ExecuteNonQuery();

            context.Result = result;
        }
        finally
        {
            connection.Close();
            connection.Dispose();
        }
    }

    private static async Task<IDataReader> ExecuteReaderAsync(IDbCommand command, CancellationToken ct)
    {
        if (command is DbCommand dbCmd)
            return await dbCmd.ExecuteReaderAsync(ct).ConfigureAwait(false);

        return command.ExecuteReader();
    }

    private static void ApplyParameters(IDbCommand command, object? parameters)
    {
        if (parameters == null) return;

        if (parameters is IDictionary<string, object> dict)
        {
            foreach (var kvp in dict)
            {
                var param = command.CreateParameter();
                param.ParameterName = $"@{kvp.Key}";
                param.Value = kvp.Value ?? DBNull.Value;
                command.Parameters.Add(param);
            }
            return;
        }

        foreach (var prop in parameters.GetType().GetProperties())
        {
            var param = command.CreateParameter();
            param.ParameterName = $"@{prop.Name}";
            param.Value = prop.GetValue(parameters) ?? DBNull.Value;
            command.Parameters.Add(param);
        }
    }
}
