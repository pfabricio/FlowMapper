namespace FlowMapper.Diagnostics.Pipeline;

public interface IDiagnosticsPipeline
{
    void Emit(DiagnosticEvent @event);
    IReadOnlyList<DiagnosticEvent> GetEvents(string? category = null);
    IDisposable BeginScope(string category);
}

public sealed record DiagnosticsScope(string Category, DateTime StartedAt) : IDisposable
{
    private readonly DiagnosticsPipeline? _pipeline;
    private bool _disposed;

    public DiagnosticsScope(DiagnosticsPipeline pipeline, string category)
        : this(category, DateTime.UtcNow)
    {
        _pipeline = pipeline;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _pipeline?.Emit(new DiagnosticEvent
        {
            Category = Category,
            Message = "Scope completed",
            Severity = DiagnosticSeverity.Info,
            Duration = DateTime.UtcNow - StartedAt
        });
    }
}
