namespace FlowMapper.Core;

public class FlattenPath
{
    public string ColumnName { get; init; } = string.Empty;
    public List<string> PathSegments { get; init; } = new();

    public string PropertyPath => string.Join(".", PathSegments);
}
