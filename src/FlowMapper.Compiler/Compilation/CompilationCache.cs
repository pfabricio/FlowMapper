using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using FlowMapper.Execution;

namespace FlowMapper.Compiler.Compilation;

public sealed class CompilationCache : ICompilationCache
{
    private readonly ConcurrentDictionary<CompilationKey, object> _cache = new();

    public bool TryGet<T>(CompilationKey key, [MaybeNullWhen(false)] out T value)
    {
        if (_cache.TryGetValue(key, out var raw) && raw is T typed)
        {
            value = typed;
            return true;
        }
        value = default;
        return false;
    }

    public void Store<T>(CompilationKey key, T value)
    {
        _cache[key] = value!;
    }

    public void Invalidate(CompilationKey key)
    {
        _cache.TryRemove(key, out _);
    }

    public void Clear()
    {
        _cache.Clear();
    }

    public int Count => _cache.Count;

    public IReadOnlyCollection<CompilationKey> Keys => _cache.Keys.ToList();

    public bool ContainsKey(CompilationKey key) => _cache.ContainsKey(key);
}
