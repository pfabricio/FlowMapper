namespace FlowMapper.Core;

/// <summary>Represents a nested mapping between two complex type properties.
/// Contains the parent property name and the child <c>Flow</c> that defines
/// how the nested type is mapped recursively.</summary>
public class NestedFlow
{
    /// <summary>The destination property name that holds the nested type.</summary>
    public string ParentProperty { get; set; } = string.Empty;

    /// <summary>The child flow defining how the nested type's properties are mapped.</summary>
    public Flow ChildFlow { get; set; } = new();

    /// <summary>The resolution strategy for this nested mapping.</summary>
    public MappingStrategy Strategy { get; set; }
}
