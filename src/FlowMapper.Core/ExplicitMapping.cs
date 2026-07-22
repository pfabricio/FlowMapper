namespace FlowMapper.Core;

public class ExplicitMapping
{
    public string SourceProperty { get; set; } = string.Empty;
    public string DestinationProperty { get; set; } = string.Empty;
    public bool IsIgnored { get; set; }
    public bool IsPathMapping { get; set; }
    public List<string> PathSegments { get; set; } = new();
    public string? MapFromExpression { get; set; }
}
