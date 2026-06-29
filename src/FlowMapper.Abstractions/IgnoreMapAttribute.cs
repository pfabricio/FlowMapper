namespace FlowMapper.Abstractions;

/// <summary>Instructs the mapper to skip the specified destination property during mapping.
/// Applied to a mapper class, this attribute prevents convention-based matching for the named property.</summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class IgnoreMapAttribute : Attribute
{
    /// <summary>The name of the destination property to ignore.</summary>
    public string PropertyName { get; }

    /// <summary>Creates a new ignore mapping instruction.</summary>
    /// <param name="propertyName">Destination property name to exclude from mapping.</param>
    public IgnoreMapAttribute(string propertyName)
    {
        PropertyName = propertyName;
    }
}
