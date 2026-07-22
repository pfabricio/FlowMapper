namespace FlowMapper.Execution;

public class ExecutionGraph
{
    public List<ExecutionNode> Nodes { get; init; } = new();
    public List<ExecutionEdge> Edges { get; init; } = new();
}

public class ExecutionEdge
{
    public ExecutionNode From { get; init; } = null!;
    public ExecutionNode To { get; init; } = null!;
}
