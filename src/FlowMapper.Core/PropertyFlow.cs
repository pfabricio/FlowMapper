namespace FlowMapper.Core;

/// <summary>Describes how a single destination property is mapped from a source property.
/// Includes the source path resolution strategy (Direct, Constructor, Nested, or Flatten).</summary>
public class PropertyFlow
{
    /// <summary>The name of the source property (or the first segment of a flattened/nested path).</summary>
    public string SourceProperty { get; set; } = string.Empty;

    /// <summary>The name of the destination property.</summary>
    public string DestinationProperty { get; set; } = string.Empty;

    /// <summary>For flatten/constructor strategies, the full source path (e.g., "Address.Street").</summary>
    public string SourcePath { get; set; } = string.Empty;

    /// <summary>The resolution strategy used for this property mapping.</summary>
    public MappingStrategy Strategy { get; set; }

    /// <summary>When mapping via constructor, the index of the matching constructor parameter.</summary>
    public int? ConstructorParameterIndex { get; set; }

    /// <summary>Custom expression body for compile-time evaluation (e.g., "source.FirstName + \" \" + source.LastName").</summary>
    public string? MapFromExpression { get; set; }
}

/// <summary>Defines how a property value is resolved from source during mapping.</summary>
public enum MappingStrategy
{
    /// <summary>Direct property-to-property mapping (same name and type).</summary>
    Direct,

    /// <summary>Mapping via a constructor parameter (immutable types).</summary>
    Constructor,

    /// <summary>Property maps to a nested complex type with its own sub-flow.</summary>
    Nested,

    /// <summary>Property maps via a flattened path through intermediate properties (e.g., Address.Street to AddressStreet).</summary>
    Flatten
}
