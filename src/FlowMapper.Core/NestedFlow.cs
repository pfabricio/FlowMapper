namespace FlowMapper.Core;

public class NestedFlow
{
    public string ParentProperty { get; init; } = string.Empty;
    public Flow ChildFlow { get; init; } = null!;
    public MappingStrategy Strategy { get; set; } = MappingStrategy.Auto;
}
