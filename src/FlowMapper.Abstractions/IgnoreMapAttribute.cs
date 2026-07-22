namespace FlowMapper.Abstractions;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class IgnoreMapAttribute : Attribute
{
    public string PropertyName { get; }

    public IgnoreMapAttribute(string propertyName)
    {
        PropertyName = propertyName;
    }
}
