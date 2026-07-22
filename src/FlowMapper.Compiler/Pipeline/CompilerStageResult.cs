namespace FlowMapper.Compiler.Pipeline;

public sealed record CompilerStageResult(
    bool Success,
    IReadOnlyList<CompilerDiagnostic> Diagnostics,
    TimeSpan Duration);

public sealed record CompilerDiagnostic(
    string Stage,
    string Message,
    CompilerDiagnosticSeverity Severity,
    string? Detail = null);

public enum CompilerDiagnosticSeverity
{
    Info,
    Warning,
    Error
}
