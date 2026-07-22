using System.Data;
using FlowMapper.Abstractions;
using FlowMapper.Execution;

namespace FlowMapper.Runtime;

public class Runtime : IRuntime
{
    public async Task<TDest> ExecuteAsync<TSource, TDest>(
        TSource source, ExecutionArtifact artifact, CancellationToken ct = default)
    {
        await Task.CompletedTask;
        throw new NotImplementedException("Runtime execution requires generated mapper or IMapper<,> implementation.");
    }
}
