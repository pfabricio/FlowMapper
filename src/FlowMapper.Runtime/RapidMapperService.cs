using System.Runtime.CompilerServices;
using FlowMapper.Abstractions;
using FlowMapper.Providers.Abstractions;

namespace FlowMapper.Runtime;

public class RapidMapperService : IRapidMapper
{
    private readonly IQueryExecutor _queryExecutor;
    private readonly ICommandExecutor _commandExecutor;
    private readonly IStreamExecutor _streamExecutor;
    private readonly IExecutionScopeFactory _scopeFactory;

    public RapidMapperService(
        IQueryExecutor queryExecutor,
        ICommandExecutor commandExecutor,
        IStreamExecutor streamExecutor,
        IExecutionScopeFactory scopeFactory)
    {
        _queryExecutor = queryExecutor;
        _commandExecutor = commandExecutor;
        _streamExecutor = streamExecutor;
        _scopeFactory = scopeFactory;
    }

    public Task<IEnumerable<T>> QueryAsync<T>(string sql, object? parameters = null, ExecutionOptions? options = null, CancellationToken ct = default)
        => _queryExecutor.QueryAsync<T>(sql, parameters, options, ct);

    public Task<T> QuerySingleAsync<T>(string sql, object? parameters = null, ExecutionOptions? options = null, CancellationToken ct = default)
    {
        return QuerySingleAsyncCore<T>(sql, parameters, options, ct);
    }

    private async Task<T> QuerySingleAsyncCore<T>(string sql, object? parameters = null, ExecutionOptions? options = null, CancellationToken ct = default)
    {
        var results = await _queryExecutor.QueryAsync<T>(sql, parameters, options, ct);
        return results.Single();
    }

    public async Task<T?> QuerySingleOrDefaultAsync<T>(string sql, object? parameters = null, ExecutionOptions? options = null, CancellationToken ct = default)
    {
        var results = await _queryExecutor.QueryAsync<T>(sql, parameters, options, ct);
        return results.SingleOrDefault();
    }

    public async Task<T> QueryScalarAsync<T>(string sql, object? parameters = null, ExecutionOptions? options = null, CancellationToken ct = default)
    {
        return await _commandExecutor.ExecuteScalarAsync<T>(sql, parameters, options, ct);
    }

    public Task<int> ExecuteAsync(string sql, object? parameters = null, ExecutionOptions? options = null, CancellationToken ct = default)
        => _commandExecutor.ExecuteAsync(sql, parameters, options, ct);

    public Task<T> ExecuteScalarAsync<T>(string sql, object? parameters = null, ExecutionOptions? options = null, CancellationToken ct = default)
        => _commandExecutor.ExecuteScalarAsync<T>(sql, parameters, options, ct);

    public IAsyncEnumerable<T> StreamAsync<T>(string sql, object? parameters = null, ExecutionOptions? options = null, CancellationToken ct = default)
        => _streamExecutor.StreamAsync<T>(sql, parameters, options, ct);

    public IExecutionScope CreateScope(bool transactional = false)
        => _scopeFactory.CreateScope(transactional);
}
