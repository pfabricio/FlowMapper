using System.Collections.Concurrent;

namespace FlowMapper.Compiler.Compilation;

public sealed class DependencyGraph : IDependencyGraph
{
    private readonly ConcurrentDictionary<CompilationKey, HashSet<CompilationKey>> _edges = new();
    private readonly ConcurrentDictionary<CompilationKey, bool> _validity = new();

    public void AddDependency(CompilationKey from, CompilationKey to)
    {
        _edges.AddOrUpdate(
            from,
            _ => [to],
            (_, deps) =>
            {
                lock (deps)
                    deps.Add(to);
                return deps;
            });
    }

    public void Invalidate(CompilationKey key)
    {
        _validity[key] = false;
    }

    public bool IsValid(CompilationKey key)
    {
        return _validity.GetOrAdd(key, true);
    }

    public IReadOnlyCollection<CompilationKey> GetDependents(CompilationKey key)
    {
        if (_edges.TryGetValue(key, out var deps))
        {
            lock (deps)
                return deps.ToList();
        }
        return [];
    }

    public void Clear()
    {
        _edges.Clear();
        _validity.Clear();
    }

    public int Count => _edges.Count;
}
