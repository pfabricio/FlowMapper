namespace FlowMapper.Execution.Artifacts;

public interface IMetadataArtifact : IExecutionArtifact
{
    IReadOnlyCollection<ITypeInfo> Types { get; }
}

public interface ITypeInfo
{
    string FullName { get; }
    string? BaseType { get; }
    IReadOnlyCollection<string> PropertyNames { get; }
}

public sealed record MetadataArtifact(
    string Name,
    Version Version,
    IReadOnlyCollection<ITypeInfo> Types
) : IMetadataArtifact;

public sealed record TypeInfo(
    string FullName,
    string? BaseType,
    IReadOnlyCollection<string> PropertyNames
) : ITypeInfo;
