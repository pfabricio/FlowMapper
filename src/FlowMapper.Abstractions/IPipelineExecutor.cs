namespace FlowMapper.Abstractions;

public interface IPipelineExecutor
{
    Task<IEnumerable<T>> QueryAsync<T>(string sql, object? parameters = null, ExecutionOptions? options = null, CancellationToken ct = default);
    Task<int> ExecuteAsync(string sql, object? parameters = null, ExecutionOptions? options = null, CancellationToken ct = default);
    IAsyncEnumerable<T> StreamAsync<T>(string sql, object? parameters = null, ExecutionOptions? options = null, CancellationToken ct = default);
}
