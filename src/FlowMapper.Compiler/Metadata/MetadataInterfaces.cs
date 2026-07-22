namespace FlowMapper.Compiler.Metadata;

public interface IMetadataModel
{
    IReadOnlyCollection<ITypeMetadata> Types { get; }
}

public interface ITypeMetadata
{
    string Name { get; }
    string Namespace { get; }
    string? BaseType { get; }
    IReadOnlyCollection<string> Interfaces { get; }
    IReadOnlyCollection<IConstructorMetadata> Constructors { get; }
    IReadOnlyCollection<IMemberMetadata> Members { get; }
    object? Tag { get; }
}

public interface IMemberMetadata
{
    string Name { get; }
    string TypeName { get; }
    bool CanRead { get; }
    bool CanWrite { get; }
    bool IsPublic { get; }
}

public interface IConstructorMetadata
{
    IReadOnlyCollection<IParameterMetadata> Parameters { get; }
    bool IsPublic { get; }
}

public interface IParameterMetadata
{
    string Name { get; }
    string TypeName { get; }
}

public interface IRelationshipMetadata
{
    string SourceType { get; }
    string TargetType { get; }
    string Kind { get; }
}

public interface IMappingMetadata
{
    string SourceType { get; }
    string TargetType { get; }
    bool HasReverseMap { get; }
    IReadOnlyCollection<IMemberMapping> Members { get; }
    IReadOnlyCollection<IPathMapping> Paths { get; }
    IReadOnlyCollection<string> IgnoredMembers { get; }
}

public interface IMemberMapping
{
    string DestinationMember { get; }
    string? SourceMember { get; }
    string? SourceExpression { get; }
}

public interface IPathMapping
{
    string DestinationPath { get; }
    string? SourceProperty { get; }
    string? SourceExpression { get; }
}

public interface IProviderMetadata
{
    string Name { get; }
    string Version { get; }
    string Category { get; }
}
