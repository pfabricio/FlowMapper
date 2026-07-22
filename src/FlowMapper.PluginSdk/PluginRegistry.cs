using System.Collections.Concurrent;

namespace FlowMapper.PluginSdk;

public interface IPluginRegistry
{
    void Register(IFlowMapperPlugin plugin);
    IReadOnlyCollection<IFlowMapperPlugin> GetAll();
    IReadOnlyCollection<T> GetByCategory<T>() where T : class;
    bool IsRegistered(string name);
    void Clear();
    int Count { get; }
}

public sealed class PluginRegistry : IPluginRegistry
{
    private readonly ConcurrentDictionary<string, IFlowMapperPlugin> _plugins = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<Type, object> _services = new();

    public void Register(IFlowMapperPlugin plugin)
    {
        _plugins[plugin.Name] = plugin;
    }

    public IReadOnlyCollection<IFlowMapperPlugin> GetAll() => _plugins.Values.ToList().AsReadOnly();

    public IReadOnlyCollection<T> GetByCategory<T>() where T : class
    {
        return _plugins.Values
            .OfType<T>()
            .ToList()
            .AsReadOnly();
    }

    public bool IsRegistered(string name) => _plugins.ContainsKey(name);

    public void Clear()
    {
        _plugins.Clear();
        _services.Clear();
    }

    public int Count => _plugins.Count;

    public void RegisterService<T>(T instance) where T : class
    {
        _services[typeof(T)] = instance;
    }

    public T? ResolveService<T>() where T : class
    {
        return _services.TryGetValue(typeof(T), out var instance) ? instance as T : null;
    }
}
