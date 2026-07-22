using FlowMapper.Execution;

namespace FlowMapper.Runtime;

public interface IRuntime
{
    Task<TDest> ExecuteAsync<TSource, TDest>(TSource source, ExecutionArtifact artifact, CancellationToken ct = default);
}
