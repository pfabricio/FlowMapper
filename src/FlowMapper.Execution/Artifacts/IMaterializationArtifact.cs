namespace FlowMapper.Execution.Artifacts;

public interface IMaterializationArtifact : IExecutionArtifact
{
    Type TargetType { get; }
    string Separator { get; }
    Delegate? ConstructorDelegate { get; }
    IReadOnlyCollection<IColumnBinding> ColumnBindings { get; }
    Delegate? MaterializationDelegate { get; }
}

public interface IColumnBinding
{
    string ColumnName { get; }
    string MemberName { get; }
    Type MemberType { get; }
    Delegate? Converter { get; }
    bool IsNullable { get; }
}

public sealed record MaterializationArtifact(
    string Name,
    Version Version,
    Type TargetType,
    string Separator,
    Delegate? ConstructorDelegate,
    IReadOnlyCollection<IColumnBinding> ColumnBindings,
    Delegate? MaterializationDelegate
) : IMaterializationArtifact;

public sealed record ColumnBinding(
    string ColumnName,
    string MemberName,
    Type MemberType,
    Delegate? Converter,
    bool IsNullable
) : IColumnBinding;
