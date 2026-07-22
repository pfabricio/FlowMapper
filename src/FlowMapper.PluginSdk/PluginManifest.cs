namespace FlowMapper.PluginSdk;

public sealed record PluginManifest(
    string Name,
    Version Version,
    string Author,
    string Description,
    IReadOnlyCollection<PluginDependency> Dependencies,
    IReadOnlyCollection<string> Capabilities);

public sealed record PluginDependency(
    string PluginName,
    Version MinVersion,
    Version? MaxVersion = null);
