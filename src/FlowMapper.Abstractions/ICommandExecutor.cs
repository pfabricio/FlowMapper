namespace FlowMapper.Abstractions;

public interface ICommandExecutor
{
    Task<int> ExecuteAsync(string sql, object? parameters = null, ExecutionOptions? options = null, CancellationToken ct = default);
    Task<T> ExecuteScalarAsync<T>(string sql, object? parameters = null, ExecutionOptions? options = null, CancellationToken ct = default);
}
