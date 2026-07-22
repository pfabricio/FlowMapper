namespace FlowMapper.Compiler.Metadata;

public sealed record MetadataModel(
    IReadOnlyCollection<ITypeMetadata> Types
) : IMetadataModel;

public sealed record TypeMetadata(
    string Name,
    string Namespace,
    string? BaseType,
    IReadOnlyCollection<string> Interfaces,
    IReadOnlyCollection<IConstructorMetadata> Constructors,
    IReadOnlyCollection<IMemberMetadata> Members,
    object? Tag = null
) : ITypeMetadata;

public sealed record MemberMetadata(
    string Name,
    string TypeName,
    bool CanRead,
    bool CanWrite,
    bool IsPublic
) : IMemberMetadata;

public sealed record ConstructorMetadata(
    IReadOnlyCollection<IParameterMetadata> Parameters,
    bool IsPublic
) : IConstructorMetadata;

public sealed record ParameterMetadata(
    string Name,
    string TypeName
) : IParameterMetadata;

public sealed record RelationshipMetadata(
    string SourceType,
    string TargetType,
    string Kind
) : IRelationshipMetadata;

public sealed record MappingMetadata(
    string SourceType,
    string TargetType,
    bool HasReverseMap,
    IReadOnlyCollection<IMemberMapping> Members,
    IReadOnlyCollection<IPathMapping> Paths,
    IReadOnlyCollection<string> IgnoredMembers
) : IMappingMetadata;

public sealed record MemberMapping(
    string DestinationMember,
    string? SourceMember,
    string? SourceExpression
) : IMemberMapping;

public sealed record PathMapping(
    string DestinationPath,
    string? SourceProperty,
    string? SourceExpression
) : IPathMapping;

public sealed record ProviderMetadata(
    string Name,
    string Version,
    string Category
) : IProviderMetadata;
