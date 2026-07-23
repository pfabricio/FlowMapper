namespace FlowMapper.Execution.Artifacts;

public interface ISqlArtifact : IExecutionArtifact
{
    string CommandText { get; }
    CommandType CommandKind { get; }
    IReadOnlyCollection<IParameterBinding> Parameters { get; }
    Delegate? ExecutionDelegate { get; }
}

public enum CommandType
{
    Query,
    NonQuery,
    Scalar,
    StoredProcedure
}

public interface IParameterBinding
{
    string Name { get; }
    Type Type { get; }
    int? Size { get; }
}

public sealed record SqlArtifact(
    string Name,
    Version Version,
    string CommandText,
    CommandType CommandKind,
    IReadOnlyCollection<IParameterBinding> Parameters,
    Delegate? ExecutionDelegate
) : ISqlArtifact;

public sealed record ParameterBinding(
    string Name,
    Type Type,
    int? Size
) : IParameterBinding;
