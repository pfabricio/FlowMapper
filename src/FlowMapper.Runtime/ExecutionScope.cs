using System.Data;
using FlowMapper.Abstractions;

namespace FlowMapper.Runtime;

public class ExecutionScope : IExecutionScope
{
    private readonly IDbConnection _connection;
    private readonly IDbTransaction? _transaction;
    private bool _committed;
    private bool _disposed;

    public IDbConnection Connection => _connection;
    public IDbTransaction? Transaction => _transaction;

    public ExecutionScope(IDbConnection connection, bool transactional)
    {
        _connection = connection;
        _connection.Open();
        if (transactional)
            _transaction = _connection.BeginTransaction();
    }

    public Task CommitAsync(CancellationToken ct = default)
    {
        if (_transaction == null)
            throw new InvalidOperationException("Scope is not transactional");
        _transaction.Commit();
        _committed = true;
        return Task.CompletedTask;
    }

    public Task RollbackAsync(CancellationToken ct = default)
    {
        _transaction?.Rollback();
        return Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;

        if (_transaction != null && !_committed)
            _transaction.Rollback();

        _transaction?.Dispose();
        _connection.Close();
        _connection.Dispose();
        await Task.CompletedTask;
    }
}
