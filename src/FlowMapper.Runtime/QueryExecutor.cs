using FlowMapper.Abstractions;

namespace FlowMapper.Runtime;

public class QueryExecutor : IQueryExecutor
{
    private readonly IPipelineExecutor _pipelineExecutor;

    public QueryExecutor(IPipelineExecutor pipelineExecutor)
    {
        _pipelineExecutor = pipelineExecutor;
    }

    public Task<IEnumerable<T>> QueryAsync<T>(string sql, object? parameters = null, ExecutionOptions? options = null, CancellationToken ct = default)
        => _pipelineExecutor.QueryAsync<T>(sql, parameters, options, ct);

    public Task<T> QuerySingleAsync<T>(string sql, object? parameters = null, ExecutionOptions? options = null, CancellationToken ct = default)
    {
        var results = _pipelineExecutor.QueryAsync<T>(sql, parameters, options, ct);
        return results.ContinueWith(t => t.Result.Single(), ct);
    }

    public async Task<T?> QuerySingleOrDefaultAsync<T>(string sql, object? parameters = null, ExecutionOptions? options = null, CancellationToken ct = default)
    {
        var results = await _pipelineExecutor.QueryAsync<T>(sql, parameters, options, ct);
        return results.SingleOrDefault();
    }

    public async Task<T> QueryScalarAsync<T>(string sql, object? parameters = null, ExecutionOptions? options = null, CancellationToken ct = default)
    {
        var results = await _pipelineExecutor.QueryAsync<T>(sql, parameters, options, ct);
        return results.First();
    }

    public IAsyncEnumerable<T> StreamAsync<T>(string sql, object? parameters = null, ExecutionOptions? options = null, CancellationToken ct = default)
        => _pipelineExecutor.StreamAsync<T>(sql, parameters, options, ct);
}
