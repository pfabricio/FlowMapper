using FlowMapper.Abstractions;

namespace FlowMapper.Diagnostics;

public class DiagnosticsService
{
    private readonly List<DiagnosticEvent> _events = new();
    private readonly object _lock = new();

    public event EventHandler<DiagnosticEvent>? OnEvent;

    public void Emit(DiagnosticEvent @event)
    {
        lock (_lock)
        {
            _events.Add(@event);
        }
        OnEvent?.Invoke(this, @event);
    }

    public IReadOnlyList<DiagnosticEvent> GetEvents()
    {
        lock (_lock)
        {
            return _events.ToList();
        }
    }
}

public class DiagnosticEvent
{
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string Category { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public DiagnosticSeverity Severity { get; init; }
    public TimeSpan? Duration { get; init; }
}

public enum DiagnosticSeverity
{
    Info,
    Warning,
    Error
}
