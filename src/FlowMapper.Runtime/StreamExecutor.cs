using System.Runtime.CompilerServices;
using FlowMapper.Abstractions;

namespace FlowMapper.Runtime;

public class StreamExecutor : IStreamExecutor
{
    private readonly IPipelineExecutor _pipelineExecutor;

    public StreamExecutor(IPipelineExecutor pipelineExecutor)
    {
        _pipelineExecutor = pipelineExecutor;
    }

    public IAsyncEnumerable<T> StreamAsync<T>(string sql, object? parameters = null, ExecutionOptions? options = null, CancellationToken ct = default)
        => _pipelineExecutor.StreamAsync<T>(sql, parameters, options, ct);
}
