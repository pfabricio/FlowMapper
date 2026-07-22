using System.Collections.Concurrent;

namespace FlowMapper.Compiler.Compilation;

public interface IChangeTracker
{
    bool HasChanged(string key, string contentHash);
    void Track(string key, string contentHash);
    void Reset();
}

public sealed class ChangeTracker : IChangeTracker
{
    private readonly ConcurrentDictionary<string, string> _snapshots = new(StringComparer.Ordinal);

    public bool HasChanged(string key, string contentHash)
    {
        return !_snapshots.TryGetValue(key, out var existing) || existing != contentHash;
    }

    public void Track(string key, string contentHash)
    {
        _snapshots[key] = contentHash;
    }

    public void Reset()
    {
        _snapshots.Clear();
    }

    public int Count => _snapshots.Count;
}
