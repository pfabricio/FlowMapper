using System;
using System.Threading.Tasks;
using FlowMapper.Abstractions;

namespace FlowMapper.Core;

public class RetryExecutionStrategy : IExecutionStrategy
{
    private readonly int _maxRetries;
    private readonly TimeSpan _initialDelay;

    public RetryExecutionStrategy(int maxRetries = 3, TimeSpan? initialDelay = null)
    {
        _maxRetries = maxRetries;
        _initialDelay = initialDelay ?? TimeSpan.FromMilliseconds(100);
    }

    public async Task ExecuteAsync(Func<Task> operation)
    {
        for (int i = 0; i <= _maxRetries; i++)
        {
            try
            {
                await operation();
                return;
            }
            catch when (i < _maxRetries)
            {
                var delay = TimeSpan.FromMilliseconds(_initialDelay.TotalMilliseconds * Math.Pow(2, i));
                await Task.Delay(delay);
            }
        }
    }
}
