namespace FlowMapper.PluginSdk;

public interface IFlowMapperPlugin
{
    string Name { get; }
    Version Version { get; }
    void Configure(IPluginBuilder builder);
}
