using Microsoft.CodeAnalysis;

namespace FlowMapper.SourceGenerator.Pipeline.Validator;

public class FlowDiagnosticResult
{
    public string Id { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsWarning { get; set; }
    public Location? Location { get; set; }

    public static FlowDiagnosticResult Warning(string id, string message)
        => new() { Id = id, Message = message, IsWarning = true };

    public static FlowDiagnosticResult Error(string id, string message)
        => new() { Id = id, Message = message, IsWarning = false };
}
