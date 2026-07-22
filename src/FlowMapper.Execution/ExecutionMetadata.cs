using FlowMapper.Abstractions;

namespace FlowMapper.Execution;

public class ExecutionMetadata
{
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public string CompilerVersion { get; init; } = "2.0.0";
    public FlowMapperOptions Options { get; init; } = new();
}
