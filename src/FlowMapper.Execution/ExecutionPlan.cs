using FlowMapper.Primitives;

namespace FlowMapper.Execution;

public class ExecutionPlan
{
    public PipelineId Id { get; init; } = PipelineId.New();
    public List<ExecutionNode> Nodes { get; init; } = new();
    public ExecutionGraph Graph { get; init; } = new();
}
