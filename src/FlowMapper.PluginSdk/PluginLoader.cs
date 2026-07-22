using System.Reflection;

namespace FlowMapper.PluginSdk;

public sealed class PluginLoader
{
    private readonly PluginRegistry _registry;
    private readonly PluginDependencyResolver _dependencyResolver;

    public PluginLoader(PluginRegistry? registry = null)
    {
        _registry = registry ?? new PluginRegistry();
        _dependencyResolver = new PluginDependencyResolver();
    }

    public PluginRegistry Registry => _registry;

    public LoadResult LoadFromAssembly(Assembly assembly)
    {
        var plugins = assembly.GetTypes()
            .Where(t => !t.IsAbstract && typeof(IFlowMapperPlugin).IsAssignableFrom(t))
            .Select(t =>
            {
                try
                {
                    return (IFlowMapperPlugin?)Activator.CreateInstance(t);
                }
                catch
                {
                    return null;
                }
            })
            .Where(p => p != null)
            .ToList()!;

        return LoadPlugins(plugins);
    }

    public LoadResult LoadPlugins(IReadOnlyList<IFlowMapperPlugin> plugins)
    {
        var loaded = new List<IFlowMapperPlugin>();
        var errors = new List<string>();

        foreach (var plugin in plugins)
        {
            if (_registry.IsRegistered(plugin.Name))
            {
                errors.Add($"Plugin '{plugin.Name}' is already registered.");
                continue;
            }

            try
            {
                var builder = new PluginBuilder(_registry);
                plugin.Configure(builder);
                _registry.Register(plugin);
                loaded.Add(plugin);
            }
            catch (Exception ex)
            {
                errors.Add($"Failed to load plugin '{plugin.Name}': {ex.Message}");
            }
        }

        return new LoadResult(loaded.AsReadOnly(), errors.AsReadOnly());
    }

    public void UnloadAll()
    {
        _registry.Clear();
    }
}

public sealed record LoadResult(
    IReadOnlyCollection<IFlowMapperPlugin> Loaded,
    IReadOnlyCollection<string> Errors)
{
    public bool Success => Errors.Count == 0;
}

internal sealed class PluginBuilder : IPluginBuilder
{
    private readonly PluginRegistry _registry;

    public PluginBuilder(PluginRegistry registry)
    {
        _registry = registry;
    }

    public IPluginBuilder AddProvider<T>() where T : class
    {
        return this;
    }

    public IPluginBuilder AddValidationRule<T>() where T : class
    {
        return this;
    }

    public IPluginBuilder AddOptimizationPass(Type passType)
    {
        return this;
    }

    public IPluginBuilder AddCompilerStage(Type stageType)
    {
        return this;
    }

    public IPluginBuilder AddSourceGenerator<T>() where T : class
    {
        return this;
    }

    public IPluginBuilder AddRuntimeService<T>(T instance) where T : class
    {
        _registry.RegisterService(instance);
        return this;
    }
}
