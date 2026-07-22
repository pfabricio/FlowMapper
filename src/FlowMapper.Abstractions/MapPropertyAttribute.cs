namespace FlowMapper.Abstractions;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class MapPropertyAttribute : Attribute
{
    public string Source { get; }

    public string Destination { get; }

    public MapPropertyAttribute(string source, string destination)
    {
        Source = source;
        Destination = destination;
    }
}
