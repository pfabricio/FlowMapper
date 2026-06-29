using FlowMapper.Abstractions;

namespace FlowMapper.Core;

/// <summary>Contains policy configuration for a mapping flow. Policies control diagnostic strictness,
/// flatten resolution, and constructor preference. Applied via <c>FlowProfileAttribute</c> or <c>MappingExpression</c>.</summary>
public class MappingPolicy
{
    /// <summary>Strictness level for unmapped property reporting.</summary>
    public StrictnessLevel Strictness { get; set; } = StrictnessLevel.None;

    /// <summary>When true, the source generator will attempt flatten resolution for unmatched properties.</summary>
    public bool EnableFlatten { get; set; } = true;

    /// <summary>When true, constructor-based mapping is preferred over property setters.</summary>
    public bool PreferConstructor { get; set; } = false;
}
