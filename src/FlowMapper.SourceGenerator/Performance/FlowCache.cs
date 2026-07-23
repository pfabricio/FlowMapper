using System.Collections.Concurrent;
using FlowMapper.SourceGenerator.Models;

namespace FlowMapper.SourceGenerator.Performance;

public class FlowCache
{
    private readonly ConcurrentDictionary<string, FlowDescriptor> _cache = new();

    public bool TryGet(string key, out FlowDescriptor? flow)
    {
        return _cache.TryGetValue(key, out flow);
    }

    public void Set(string key, FlowDescriptor flow)
    {
        _cache[key] = flow;
    }

    public void Clear()
    {
        _cache.Clear();
    }
}