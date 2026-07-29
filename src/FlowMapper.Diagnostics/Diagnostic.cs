namespace FlowMapper.Diagnostics;

public sealed record Diagnostic(
    string Code,
    DiagnosticSeverity Severity,
    string Message,
    string? Table = null,
    string? Column = null,
    string? Provider = null,
    DiagnosticSource Source = DiagnosticSource.Runtime);
