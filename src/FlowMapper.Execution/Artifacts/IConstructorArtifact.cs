namespace FlowMapper.Execution.Artifacts;

public interface IConstructorArtifact : IExecutionArtifact
{
    Type TargetType { get; }
    IReadOnlyCollection<IConstructorParameterBinding> Parameters { get; }
    Delegate? FactoryDelegate { get; }
}

public interface IConstructorParameterBinding
{
    string Name { get; }
    Type Type { get; }
    bool HasDefaultValue { get; }
    object? DefaultValue { get; }
}

public sealed record ConstructorArtifact(
    string Name,
    Version Version,
    Type TargetType,
    IReadOnlyCollection<IConstructorParameterBinding> Parameters,
    Delegate? FactoryDelegate
) : IConstructorArtifact;

public sealed record ConstructorParameterBinding(
    string Name,
    Type Type,
    bool HasDefaultValue,
    object? DefaultValue
) : IConstructorParameterBinding;
