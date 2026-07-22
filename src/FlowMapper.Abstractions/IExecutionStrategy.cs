using System.Threading.Tasks;

namespace FlowMapper.Abstractions;

public interface IExecutionStrategy
{
    Task ExecuteAsync(Func<Task> operation);
}
