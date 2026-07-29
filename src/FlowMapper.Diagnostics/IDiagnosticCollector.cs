namespace FlowMapper.Diagnostics;

public interface IDiagnosticCollector
{
    bool HasErrors { get; }
    IReadOnlyList<Diagnostic> Diagnostics { get; }
    void Emit(Diagnostic diagnostic);
    void Clear();
}
