namespace FlowMapper.Abstractions;

public interface IPipelineBehavior
{
    bool ShouldExecute<T>(ExecutionContext<T> context);
    Task HandleAsync<T>(ExecutionContext<T> context, Func<Task> next);
}
