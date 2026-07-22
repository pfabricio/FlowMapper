namespace FlowMapper.Diagnostics.Pipeline.Middlewares;

public sealed class LoggingMiddleware : IDiagnosticsMiddleware
{
    private readonly Action<string> _logAction;

    public LoggingMiddleware(Action<string>? logAction = null)
    {
        _logAction = logAction ?? Console.WriteLine;
    }

    public void Process(DiagnosticEvent @event, DiagnosticsDelegate next)
    {
        _logAction($"[{@event.Severity}] {@event.Category}: {@event.Message}");
        next(@event);
    }
}
