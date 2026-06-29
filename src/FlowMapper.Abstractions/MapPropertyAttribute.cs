namespace FlowMapper.Abstractions;

/// <summary>Specifies an explicit source-to-destination property mapping override.
/// When convention-based mapping does not produce the desired result, apply this attribute to
/// explicitly declare which source property maps to which destination property.</summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class MapPropertyAttribute : Attribute
{
    /// <summary>The name of the source property (supports dot-separated paths for nested/flatten scenarios).</summary>
    public string Source { get; }

    /// <summary>The name of the destination property.</summary>
    public string Destination { get; }

    /// <summary>Creates a new explicit property mapping.</summary>
    /// <param name="source">Source property name or path.</param>
    /// <param name="destination">Destination property name.</param>
    public MapPropertyAttribute(string source, string destination)
    {
        Source = source;
        Destination = destination;
    }
}
