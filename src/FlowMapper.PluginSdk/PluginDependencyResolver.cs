namespace FlowMapper.PluginSdk;

public sealed class PluginDependencyResolver
{
    private readonly Dictionary<string, IFlowMapperPlugin> _resolved = new(StringComparer.OrdinalIgnoreCase);

    public ResolutionResult Resolve(IReadOnlyList<IFlowMapperPlugin> plugins)
    {
        var unresolved = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var errors = new List<string>();

        foreach (var plugin in plugins)
        {
            var manifest = GetManifest(plugin);
            if (manifest == null) continue;

            foreach (var dep in manifest.Dependencies)
            {
                var dependency = plugins.FirstOrDefault(p =>
                    p.Name.Equals(dep.PluginName, StringComparison.OrdinalIgnoreCase));

                if (dependency == null)
                {
                    errors.Add($"Plugin '{plugin.Name}' requires '{dep.PluginName}' which is not present.");
                    continue;
                }

                if (dependency.Version < dep.MinVersion)
                {
                    errors.Add($"Plugin '{plugin.Name}' requires '{dep.PluginName}' >= {dep.MinVersion}, but found {dependency.Version}.");
                }

                if (dep.MaxVersion != null && dependency.Version > dep.MaxVersion)
                {
                    errors.Add($"Plugin '{plugin.Name}' requires '{dep.PluginName}' <= {dep.MaxVersion}, but found {dependency.Version}.");
                }
            }
        }

        return new ResolutionResult(errors.Count == 0, errors.AsReadOnly());
    }

    private static PluginManifest? GetManifest(IFlowMapperPlugin plugin)
    {
        return new PluginManifest(
            plugin.Name,
            plugin.Version,
            string.Empty,
            string.Empty,
            [],
            []);
    }
}

public sealed record ResolutionResult(
    bool Success,
    IReadOnlyCollection<string> Errors);
