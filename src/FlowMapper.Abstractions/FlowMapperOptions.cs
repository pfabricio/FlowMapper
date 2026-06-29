namespace FlowMapper.Abstractions;

/// <summary>Global options for the FlowMapper framework. Passed via <c>AddFlowMapper(cfg => ...)</c>
/// during application startup to configure default mapping behavior and caching.</summary>
public class FlowMapperOptions
{
    /// <summary>The name of the default profile used when no profile attribute is specified.</summary>
    public string DefaultProfile { get; set; } = "Default";

    /// <summary>When true, flatten mapping (nested to flat property resolution) is enabled globally.</summary>
    public bool EnableFlatten { get; set; } = true;

    /// <summary>When true, constructor-based mapping is preferred over property setters globally.</summary>
    public bool PreferConstructorMapping { get; set; } = false;

    /// <summary>Global strictness level for reporting unmapped properties.</summary>
    public StrictnessLevel Strictness { get; set; } = StrictnessLevel.None;

    /// <summary>When true, generated mappers use a runtime cache to avoid redundant lookups.</summary>
    public bool EnableCache { get; set; } = true;
}
