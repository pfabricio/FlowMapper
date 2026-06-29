namespace FlowMapper.Abstractions;

/// <summary>Defines a named mapping profile. Profiles group related mappings and apply consistent
/// policy settings (flatten, constructor preference, strictness) across all mappers in the profile.</summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly)]
public sealed class FlowProfileAttribute : Attribute
{
    /// <summary>The profile name used to group related mappings.</summary>
    public string Name { get; }

    /// <summary>When enabled, the source generator searches nested properties for leaf paths
    /// that can be mapped to flat destination properties (e.g., Address.Street to AddressStreet).</summary>
    public bool EnableFlatten { get; set; } = true;

    /// <summary>When enabled, the source generator prefers constructor-based mapping
    /// over property setter mapping for immutable types.</summary>
    public bool PreferConstructor { get; set; } = false;

    /// <summary>Controls how strictly unmapped properties are reported: None allows partial mapping,
    /// Warning produces diagnostics, Error fails the build.</summary>
    public StrictnessLevel Strictness { get; set; } = StrictnessLevel.None;

    /// <summary>Creates a new profile with the specified name.</summary>
    /// <param name="name">The profile name.</param>
    public FlowProfileAttribute(string name)
    {
        Name = name;
    }
}
