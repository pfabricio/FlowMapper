namespace FlowMapper.Core;

public class PropertyFlow
{
    public string SourceProperty { get; init; } = string.Empty;
    public string DestinationProperty { get; init; } = string.Empty;
    public Type SourceType { get; init; } = null!;
    public Type DestinationType { get; init; } = null!;
    public bool IsIgnored { get; set; }
    public string? MapFromExpression { get; set; }
    public bool IsPathMapping { get; set; }
    public List<string> PathSegments { get; init; } = new();
}
