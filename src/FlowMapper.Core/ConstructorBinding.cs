namespace FlowMapper.Core;

/// <summary>Describes how a source property maps to a destination constructor parameter.
/// Used when the destination type is immutable and requires constructor-based initialization.</summary>
public class ConstructorBinding
{
    /// <summary>The name of the constructor parameter.</summary>
    public string ParameterName { get; set; } = string.Empty;

    /// <summary>The name of the source property providing the value.</summary>
    public string SourceProperty { get; set; } = string.Empty;

    /// <summary>The zero-based index of the constructor parameter.</summary>
    public int Index { get; set; }
}
