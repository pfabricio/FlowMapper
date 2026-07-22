namespace FlowMapper.Diagnostics.Pipeline.Middlewares;

public sealed class MetricsMiddleware : IDiagnosticsMiddleware
{
    private int _totalEvents;
    private int _errorCount;
    private int _warningCount;

    public int TotalEvents => _totalEvents;
    public int ErrorCount => _errorCount;
    public int WarningCount => _warningCount;

    public void Process(DiagnosticEvent @event, DiagnosticsDelegate next)
    {
        Interlocked.Increment(ref _totalEvents);

        if (@event.Severity == DiagnosticSeverity.Error)
            Interlocked.Increment(ref _errorCount);
        else if (@event.Severity == DiagnosticSeverity.Warning)
            Interlocked.Increment(ref _warningCount);

        next(@event);
    }
}
