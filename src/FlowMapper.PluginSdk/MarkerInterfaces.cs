namespace FlowMapper.PluginSdk;

public interface ICompilerPlugin : IFlowMapperPlugin
{
    IReadOnlyCollection<Type> GetStageTypes();
}

public interface IProviderPlugin : IFlowMapperPlugin { }

public interface IRuntimePlugin : IFlowMapperPlugin { }

public interface IValidationPlugin : IFlowMapperPlugin { }

public interface IOptimizationPlugin : IFlowMapperPlugin
{
    IReadOnlyCollection<Type> GetOptimizationPassTypes();
}

public interface IDiagnosticsPlugin : IFlowMapperPlugin { }

public interface ISourceGeneratorPlugin : IFlowMapperPlugin { }
