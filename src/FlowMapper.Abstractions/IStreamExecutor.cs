namespace FlowMapper.Abstractions;

public interface IStreamExecutor
{
    IAsyncEnumerable<T> StreamAsync<T>(string sql, object? parameters = null, ExecutionOptions? options = null, CancellationToken ct = default);
}
