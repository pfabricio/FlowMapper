using FlowMapper.Core;

namespace FlowMapper.SourceGenerator.Pipeline.Generator;

public class CodeWriterContext
{
    public Flow Flow { get; init; } = null!;
    public string MapperName { get; init; } = string.Empty;
    public bool IsNested { get; init; }
    public string? MethodName { get; init; }
}