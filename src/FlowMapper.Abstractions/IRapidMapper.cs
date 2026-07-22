namespace FlowMapper.Abstractions;

public interface IRapidMapper
{
    Task<IEnumerable<T>> QueryAsync<T>(string sql, object? parameters = null, ExecutionOptions? options = null, CancellationToken ct = default);
    Task<T> QuerySingleAsync<T>(string sql, object? parameters = null, ExecutionOptions? options = null, CancellationToken ct = default);
    Task<T?> QuerySingleOrDefaultAsync<T>(string sql, object? parameters = null, ExecutionOptions? options = null, CancellationToken ct = default);
    Task<T> QueryScalarAsync<T>(string sql, object? parameters = null, ExecutionOptions? options = null, CancellationToken ct = default);
    Task<int> ExecuteAsync(string sql, object? parameters = null, ExecutionOptions? options = null, CancellationToken ct = default);
    Task<T> ExecuteScalarAsync<T>(string sql, object? parameters = null, ExecutionOptions? options = null, CancellationToken ct = default);
    IAsyncEnumerable<T> StreamAsync<T>(string sql, object? parameters = null, ExecutionOptions? options = null, CancellationToken ct = default);
    IExecutionScope CreateScope(bool transactional = false);
}
