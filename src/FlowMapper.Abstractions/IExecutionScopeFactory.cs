namespace FlowMapper.Abstractions;

public interface IExecutionScopeFactory
{
    IExecutionScope CreateScope(bool transactional = false);
}
