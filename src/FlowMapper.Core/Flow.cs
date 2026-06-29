namespace FlowMapper.Core;

/// <summary>Represents a complete mapping definition between a source and destination type.
/// Contains property flows, nested flows, constructor bindings, and policy settings.
/// Built by <c>FlowBuilder</c> and consumed by <c>FlowCodeGenerator</c> to emit the mapper implementation.</summary>
public class Flow
{
    /// <summary>The fully qualified name of the source type.</summary>
    public string SourceType { get; set; } = string.Empty;

    /// <summary>The fully qualified name of the destination type.</summary>
    public string DestinationType { get; set; } = string.Empty;

    /// <summary>The profile this mapping belongs to, or "Default" if none.</summary>
    public string ProfileName { get; set; } = "Default";

    /// <summary>The list of direct and flattened property mappings.</summary>
    public List<PropertyFlow> Properties { get; set; } = new();

    /// <summary>The list of nested (complex type) mappings within this flow.</summary>
    public List<NestedFlow> NestedFlows { get; set; } = new();

    /// <summary>Constructor parameter bindings used when mapping to immutable types.</summary>
    public List<ConstructorBinding> ConstructorBindings { get; set; } = new();

    /// <summary>Policy settings (strictness, flatten, constructor) applied to this mapping.</summary>
    public MappingPolicy Policy { get; set; } = new();

    /// <summary>Optional method name to call after mapping (e.g., custom calculations).</summary>
    public string? AfterMapMethod { get; set; }

    /// <summary>Optional method name to use for constructing the destination object.</summary>
    public string? ConstructUsingMethod { get; set; }
}
