using FlowMapper.Primitives;

namespace FlowMapper.Execution;

public class ExecutionNode
{
    public string Name { get; init; } = string.Empty;
    public NodeType Type { get; init; }
    public Dictionary<string, object?> Metadata { get; init; } = new();
}

public enum NodeType
{
    Source,
    Transformation,
    Destination,
    Materialization
}
