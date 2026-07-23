using Microsoft.CodeAnalysis;

namespace FlowMapper.SourceGenerator.Models;

public class MapperDefinition
{
    public INamedTypeSymbol SourceType { get; init; } = null!;
    public INamedTypeSymbol DestinationType { get; init; } = null!;
    public INamedTypeSymbol MapperType { get; init; } = null!;
    public AttributeData Attribute { get; init; } = null!;
    public string ProfileName { get; init; } = "Default";
    public MappingPolicyModel? ProfilePolicy { get; init; }
    public HashSet<string> IgnoredProperties { get; init; } = new();
    public List<ExplicitMappingInfo> ExplicitMappings { get; init; } = new();
    public string? AfterMapMethod { get; init; }
    public string? ConstructUsingMethod { get; init; }
    public string? MapperName { get; init; }
}

public class ExplicitMappingInfo
{
    public string SourceProperty { get; set; } = string.Empty;
    public string DestinationProperty { get; set; } = string.Empty;
    public string? MapFromExpression { get; set; }
    public bool IsIgnored { get; set; }
}