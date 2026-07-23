using FlowMapper.SourceGenerator.Models;

namespace FlowMapper.SourceGenerator.Pipeline.Generator;

public class CodeWriterContext
{
    public FlowDescriptor Flow { get; init; } = null!;
    public string MapperName { get; init; } = string.Empty;
    public bool IsNested { get; init; }
    public string? MethodName { get; init; }
}