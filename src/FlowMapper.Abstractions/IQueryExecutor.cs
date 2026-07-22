namespace FlowMapper.Abstractions;

public interface IQueryExecutor
{
    Task<IEnumerable<T>> QueryAsync<T>(string sql, object? parameters = null, ExecutionOptions? options = null, CancellationToken ct = default);
    Task<T> QuerySingleAsync<T>(string sql, object? parameters = null, ExecutionOptions? options = null, CancellationToken ct = default);
    Task<T?> QuerySingleOrDefaultAsync<T>(string sql, object? parameters = null, ExecutionOptions? options = null, CancellationToken ct = default);
    Task<T> QueryScalarAsync<T>(string sql, object? parameters = null, ExecutionOptions? options = null, CancellationToken ct = default);
    IAsyncEnumerable<T> StreamAsync<T>(string sql, object? parameters = null, ExecutionOptions? options = null, CancellationToken ct = default);
}
