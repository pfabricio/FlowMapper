using FlowMapper.Primitives;

namespace FlowMapper.Core;

public class Flow
{
    public ArtifactId Id { get; init; } = ArtifactId.New();
    public string Name { get; set; } = string.Empty;
    public FlowSignature Signature { get; init; } = null!;
    public MappingStrategy Strategy { get; set; } = MappingStrategy.Auto;
    public MappingPolicy? Policy { get; set; }
    public List<PropertyFlow> Properties { get; init; } = new();
    public List<NestedFlow> NestedFlows { get; init; } = new();
    public ConstructorBindingInfo? ConstructorBinding { get; set; }
    public bool IsReverse { get; set; }
}
