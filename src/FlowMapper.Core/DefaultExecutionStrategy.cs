using System.Threading.Tasks;
using FlowMapper.Abstractions;

namespace FlowMapper.Core;

public class DefaultExecutionStrategy : IExecutionStrategy
{
    public async Task ExecuteAsync(System.Func<Task> operation)
    {
        await operation();
    }
}
