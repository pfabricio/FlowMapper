using System.Collections.Concurrent;
using FlowMapper.Core;

namespace FlowMapper.SourceGenerator.Performance;

public class FlowCache
{
    private readonly ConcurrentDictionary<string, Flow> _cache = new();

    public bool TryGet(string key, out Flow? flow)
    {
        return _cache.TryGetValue(key, out flow);
    }

    public void Set(string key, Flow flow)
    {
        _cache[key] = flow;
    }

    public void Clear()
    {
        _cache.Clear();
    }
}
