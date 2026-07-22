namespace FlowMapper.Diagnostics.Pipeline;

public sealed class DiagnosticsPipeline : IDiagnosticsPipeline, IDisposable
{
    private readonly List<DiagnosticEvent> _events = new();
    private readonly IReadOnlyList<IDiagnosticsMiddleware> _middlewares;
    private readonly object _lock = new();
    private bool _disposed;

    public event EventHandler<DiagnosticEvent>? OnEvent;

    public DiagnosticsPipeline(IEnumerable<IDiagnosticsMiddleware>? middlewares = null)
    {
        _middlewares = (middlewares as IReadOnlyList<IDiagnosticsMiddleware> ?? middlewares?.ToList()) ?? [];
    }

    public void Emit(DiagnosticEvent @event)
    {
        if (_disposed) return;

        DiagnosticsDelegate core = evt =>
        {
            lock (_lock)
            {
                _events.Add(evt);
            }
            OnEvent?.Invoke(this, evt);
        };

        var pipeline = BuildPipeline(core);
        pipeline(@event);
    }

    public IReadOnlyList<DiagnosticEvent> GetEvents(string? category = null)
    {
        lock (_lock)
        {
            if (category == null)
                return _events.ToList();

            return _events.Where(e => e.Category == category).ToList();
        }
    }

    public IDisposable BeginScope(string category)
    {
        Emit(new DiagnosticEvent
        {
            Category = category,
            Message = "Scope started",
            Severity = DiagnosticSeverity.Info
        });
        return new DiagnosticsScope(this, category);
    }

    public void Dispose()
    {
        _disposed = true;
        _events.Clear();
    }

    private DiagnosticsDelegate BuildPipeline(DiagnosticsDelegate core)
    {
        DiagnosticsDelegate pipeline = core;
        for (var i = _middlewares.Count - 1; i >= 0; i--)
        {
            var middleware = _middlewares[i];
            var current = pipeline;
            pipeline = evt => middleware.Process(evt, current);
        }
        return pipeline;
    }
}
