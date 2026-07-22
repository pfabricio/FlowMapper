using FlowMapper.Abstractions;
using FlowMapper.Primitives;

namespace FlowMapper.Execution;

public class ExecutionArtifact
{
    public ArtifactId Id { get; init; } = ArtifactId.New();
    public string Name { get; init; } = string.Empty;
    public Type SourceType { get; init; } = null!;
    public Type DestinationType { get; init; } = null!;
    public ExecutionPlan Plan { get; init; } = null!;
    public MaterializationPlan? MaterializationPlan { get; set; }
}
