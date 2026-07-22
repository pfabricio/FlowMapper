namespace FlowMapper.Abstractions;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly)]
public sealed class FlowProfileAttribute : Attribute
{
    public string Name { get; }

    public bool EnableFlatten { get; set; } = true;

    public bool PreferConstructor { get; set; } = false;

    public StrictnessLevel Strictness { get; set; } = StrictnessLevel.None;

    public FlowProfileAttribute(string name)
    {
        Name = name;
    }
}
