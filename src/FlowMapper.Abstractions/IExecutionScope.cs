namespace FlowMapper.Abstractions;

public interface IExecutionScope : IAsyncDisposable
{
    Task CommitAsync(CancellationToken ct = default);
    Task RollbackAsync(CancellationToken ct = default);
}
