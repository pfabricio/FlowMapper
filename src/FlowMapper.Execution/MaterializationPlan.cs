namespace FlowMapper.Execution;

public class MaterializationPlan
{
    public Type TargetType { get; init; } = null!;
    public List<MaterializationBinding> Bindings { get; init; } = new();
    public ConstructorBinding? Constructor { get; set; }
}

public class MaterializationBinding
{
    public string ColumnName { get; init; } = string.Empty;
    public string PropertyName { get; init; } = string.Empty;
    public Type PropertyType { get; init; } = null!;
    public bool IsNested { get; init; }
    public MaterializationPlan? NestedPlan { get; set; }
}

public class ConstructorBinding
{
    public Type Type { get; init; } = null!;
    public List<ConstructorParameter> Parameters { get; init; } = new();
}

public class ConstructorParameter
{
    public string Name { get; init; } = string.Empty;
    public Type Type { get; init; } = null!;
    public string? ColumnName { get; set; }
    public object? DefaultValue { get; set; }
}
