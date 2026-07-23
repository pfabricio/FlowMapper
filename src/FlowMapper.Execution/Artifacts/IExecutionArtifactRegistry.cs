using FlowMapper.Primitives;

namespace FlowMapper.Execution.Artifacts;

public interface IExecutionArtifactRegistry
{
    IReadOnlyCollection<IExecutionArtifact> Artifacts { get; }
}

public sealed record ExecutionArtifactRegistry(
    IReadOnlyCollection<IExecutionArtifact> Artifacts
) : IExecutionArtifactRegistry;
