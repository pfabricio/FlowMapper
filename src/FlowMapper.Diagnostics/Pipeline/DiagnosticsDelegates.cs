namespace FlowMapper.Diagnostics.Pipeline;

public delegate void DiagnosticsDelegate(DiagnosticEvent @event);

public interface IDiagnosticsMiddleware
{
    void Process(DiagnosticEvent @event, DiagnosticsDelegate next);
}
