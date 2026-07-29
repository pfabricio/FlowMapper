namespace FlowMapper.Diagnostics;

public interface IDiagnosticTelemetry
{
    void Record(Diagnostic diagnostic);
    int GetCount(string code);
    IReadOnlyDictionary<string, int> GetAllCounts();
    event Action<Diagnostic>? OnDiagnostic;
    void Reset();
}
