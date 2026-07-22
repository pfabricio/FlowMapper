using System.Data;
using System.Data.Common;
using System.Runtime.CompilerServices;
using FlowMapper.Abstractions;
using FlowMapper.Execution.Artifacts;
using FlowMapper.Materializer.Pipeline;
using FlowMapper.Providers.Abstractions;

namespace FlowMapper.Runtime;

public class DataExecutionPipeline : IQueryExecutor, ICommandExecutor, IStreamExecutor
{
    private readonly IProviderRegistry _providerRegistry;
    private readonly IMaterializationPipeline _materializationPipeline;
    private readonly string _defaultProviderName;
    private readonly string _separator;

    public DataExecutionPipeline(
        IProviderRegistry providerRegistry,
        IMaterializationPipeline materializationPipeline,
        string? separator = "_",
        string defaultProviderName = "SqlServer")
    {
        _providerRegistry = providerRegistry;
        _materializationPipeline = materializationPipeline;
        _separator = separator ?? "_";
        _defaultProviderName = defaultProviderName;
    }

    public async Task<IEnumerable<T>> QueryAsync<T>(
        string sql, object? parameters = null,
        ExecutionOptions? options = null, CancellationToken ct = default)
    {
        var provider = ResolveProvider(options);
        var plan = FlowMapper.Materializer.Materializer.BuildPlanFlat<T>();
        var mArtifact = ConvertToMaterializationArtifact<T>(plan);
        var connection = provider.CreateConnection();

        try
        {
            connection.Open();
            using var command = BuildCommand(provider, sql, parameters, options, connection);
            using var reader = await ExecuteReaderAsync(command, ct).ConfigureAwait(false);

            return _materializationPipeline.MaterializeAll<T>(reader, mArtifact).ToList();
        }
        finally
        {
            connection.Close();
            connection.Dispose();
        }
    }

    public async Task<T> QuerySingleAsync<T>(
        string sql, object? parameters = null,
        ExecutionOptions? options = null, CancellationToken ct = default)
    {
        var results = await QueryAsync<T>(sql, parameters, options, ct).ConfigureAwait(false);
        return results.Single();
    }

    public async Task<T?> QuerySingleOrDefaultAsync<T>(
        string sql, object? parameters = null,
        ExecutionOptions? options = null, CancellationToken ct = default)
    {
        var results = await QueryAsync<T>(sql, parameters, options, ct).ConfigureAwait(false);
        return results.SingleOrDefault();
    }

    public async Task<T> QueryScalarAsync<T>(
        string sql, object? parameters = null,
        ExecutionOptions? options = null, CancellationToken ct = default)
    {
        var provider = ResolveProvider(options);
        var connection = provider.CreateConnection();

        try
        {
            connection.Open();
            using var command = BuildCommand(provider, sql, parameters, options, connection);

            object? result;
            if (command is DbCommand dbCmd)
                result = await dbCmd.ExecuteScalarAsync(ct).ConfigureAwait(false);
            else
                result = command.ExecuteScalar();

            return (T)Convert.ChangeType(result, typeof(T))!;
        }
        finally
        {
            connection.Close();
            connection.Dispose();
        }
    }

    public async Task<int> ExecuteAsync(
        string sql, object? parameters = null,
        ExecutionOptions? options = null, CancellationToken ct = default)
    {
        var provider = ResolveProvider(options);
        var connection = provider.CreateConnection();

        try
        {
            connection.Open();
            using var command = BuildCommand(provider, sql, parameters, options, connection);

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

    public async Task<T> ExecuteScalarAsync<T>(
        string sql, object? parameters = null,
        ExecutionOptions? options = null, CancellationToken ct = default)
    {
        return await QueryScalarAsync<T>(sql, parameters, options, ct).ConfigureAwait(false);
    }

    public async IAsyncEnumerable<T> StreamAsync<T>(
        string sql, object? parameters = null,
        ExecutionOptions? options = null, [EnumeratorCancellation] CancellationToken ct = default)
    {
        var provider = ResolveProvider(options);
        var plan = FlowMapper.Materializer.Materializer.BuildPlanFlat<T>();
        var mArtifact = ConvertToMaterializationArtifact<T>(plan);
        var connection = provider.CreateConnection();

        try
        {
            connection.Open();
            using var command = BuildCommand(provider, sql, parameters, options, connection);
            using var reader = await ExecuteReaderAsync(command, ct).ConfigureAwait(false);

            foreach (var item in _materializationPipeline.MaterializeAll<T>(reader, mArtifact))
                yield return item;
        }
        finally
        {
            connection.Close();
            connection.Dispose();
        }
    }

    private static async Task<IDataReader> ExecuteReaderAsync(
        IDbCommand command, CancellationToken ct)
    {
        if (command is DbCommand dbCmd)
            return await dbCmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
        return command.ExecuteReader();
    }

    private static IDbCommand BuildCommand(
        IDatabaseProvider provider,
        string sql, object? parameters,
        ExecutionOptions? options, IDbConnection connection)
    {
        var cmd = connection.CreateCommand();
        cmd.CommandText = sql;
        cmd.CommandType = options?.CommandType ?? System.Data.CommandType.Text;
        if (options?.Timeout.HasValue == true)
            cmd.CommandTimeout = options.Timeout.Value;

        if (parameters != null)
            ApplyParameters(cmd, parameters);

        return cmd;
    }

    private IDatabaseProvider ResolveProvider(ExecutionOptions? options)
    {
        return _providerRegistry.GetProvider(_defaultProviderName);
    }

    private IMaterializationArtifact ConvertToMaterializationArtifact<T>(
        Execution.MaterializationPlan plan)
    {
        var bindings = plan.Bindings.Select(b => new ColumnBinding(
            b.ColumnName,
            b.PropertyName,
            b.PropertyType,
            Converter: null,
            IsNullable: Nullable.GetUnderlyingType(b.PropertyType) != null || !b.PropertyType.IsValueType
        )).ToArray();

        return new MaterializationArtifact(
            Name: $"Materialize_{typeof(T).Name}",
            Version: new Version(2, 0),
            TargetType: typeof(T),
            Separator: _separator,
            ConstructorDelegate: null,
            ColumnBindings: bindings,
            MaterializationDelegate: null
        );
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
