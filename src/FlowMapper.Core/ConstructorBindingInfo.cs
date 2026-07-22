namespace FlowMapper.Core;

public class ConstructorBindingInfo
{
    public bool UseConstructor { get; set; }
    public List<ConstructorParameterBinding> Parameters { get; init; } = new();
}

public class ConstructorParameterBinding
{
    public string ParameterName { get; init; } = string.Empty;
    public Type ParameterType { get; init; } = null!;
    public string SourceProperty { get; init; } = string.Empty;
}
