using System.Collections.Concurrent;

namespace FlowMapper.Diagnostics;

public sealed class DiagnosticTelemetry : IDiagnosticTelemetry
{
    private readonly ConcurrentDictionary<string, int> _counts = new();

    public event Action<Diagnostic>? OnDiagnostic;

    public void Record(Diagnostic diagnostic)
    {
        _counts.AddOrUpdate(diagnostic.Code, _ => 1, (_, count) => count + 1);
        OnDiagnostic?.Invoke(diagnostic);
    }

    public int GetCount(string code)
        => _counts.TryGetValue(code, out var count) ? count : 0;

    public IReadOnlyDictionary<string, int> GetAllCounts()
        => new Dictionary<string, int>(_counts);

    public void Reset() => _counts.Clear();
}
