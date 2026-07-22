namespace FlowMapper.PluginSdk;

public interface IPluginBuilder
{
    IPluginBuilder AddProvider<T>() where T : class;
    IPluginBuilder AddValidationRule<T>() where T : class;
    IPluginBuilder AddOptimizationPass(Type passType);
    IPluginBuilder AddCompilerStage(Type stageType);
    IPluginBuilder AddSourceGenerator<T>() where T : class;
    IPluginBuilder AddRuntimeService<T>(T instance) where T : class;
}
