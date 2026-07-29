namespace FlowMapper.Diagnostics;

public class DiagnosticCollector : IDiagnosticCollector
{
    private readonly List<Diagnostic> _diagnostics = new();
    private readonly object _lock = new();
    private readonly IDiagnosticTelemetry? _telemetry;

    public DiagnosticCollector()
    {
    }

    public DiagnosticCollector(IDiagnosticTelemetry telemetry)
    {
        _telemetry = telemetry;
    }

    public bool HasErrors
    {
        get { lock (_lock) return _diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error); }
    }

    public IReadOnlyList<Diagnostic> Diagnostics
    {
        get { lock (_lock) return _diagnostics.ToList(); }
    }

    public void Emit(Diagnostic diagnostic)
    {
        lock (_lock) _diagnostics.Add(diagnostic);
        _telemetry?.Record(diagnostic);
    }

    public void Clear()
    {
        lock (_lock) _diagnostics.Clear();
    }
}
