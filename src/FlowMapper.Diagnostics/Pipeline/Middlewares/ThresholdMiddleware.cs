namespace FlowMapper.Diagnostics.Pipeline.Middlewares;

public sealed class ThresholdMiddleware : IDiagnosticsMiddleware
{
    private readonly int _maxEvents;
    private int _count;

    public ThresholdMiddleware(int maxEvents = 1000)
    {
        _maxEvents = maxEvents;
    }

    public void Process(DiagnosticEvent @event, DiagnosticsDelegate next)
    {
        if (Interlocked.Increment(ref _count) > _maxEvents)
            return;

        next(@event);
    }
}
