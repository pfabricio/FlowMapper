using FlowMapper.Abstractions;
using FlowMapper.Providers.Abstractions;

namespace FlowMapper.Runtime;

public class ExecutionScopeFactory : IExecutionScopeFactory
{
    private readonly IConnectionFactory _connectionFactory;

    public ExecutionScopeFactory(IConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public IExecutionScope CreateScope(bool transactional = false)
    {
        var connection = _connectionFactory.CreateConnection();
        return new ExecutionScope(connection, transactional);
    }
}
