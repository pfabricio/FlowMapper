using FlowMapper.Abstractions;

namespace FlowMapper.Core;

public class MappingPolicy
{
    public StrictnessLevel Strictness { get; set; } = StrictnessLevel.Warning;
    public bool EnableFlatten { get; set; } = true;
    public bool PreferConstructor { get; set; }
}
