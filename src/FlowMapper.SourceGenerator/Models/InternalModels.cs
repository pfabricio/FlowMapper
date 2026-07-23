namespace FlowMapper.SourceGenerator.Models;

public class FlowDescriptor
{
    public string SourceType { get; set; } = string.Empty;
    public string DestinationType { get; set; } = string.Empty;
    public string ProfileName { get; set; } = "Default";
    public MappingPolicyModel? Policy { get; set; }
    public List<PropertyFlowModel> Properties { get; set; } = new();
    public List<NestedFlowModel> NestedFlows { get; set; } = new();
    public List<ConstructorBindingModel> ConstructorBindings { get; set; } = new();
    public string? AfterMapMethod { get; set; }
    public string? ConstructUsingMethod { get; set; }
}

public class PropertyFlowModel
{
    public string SourceProperty { get; set; } = string.Empty;
    public string DestinationProperty { get; set; } = string.Empty;
    public string? MapFromExpression { get; set; }
    public string? SourcePath { get; set; }
    public int ConstructorParameterIndex { get; set; } = -1;
    public MappingStrategy Strategy { get; set; }
}

public class NestedFlowModel
{
    public string ParentProperty { get; set; } = string.Empty;
    public FlowDescriptor ChildFlow { get; set; } = null!;
    public MappingStrategy Strategy { get; set; }
}

public class ConstructorBindingModel
{
    public string ParameterName { get; set; } = string.Empty;
    public string SourceProperty { get; set; } = string.Empty;
    public int Index { get; set; }
}

public class MappingPolicyModel
{
    public bool EnableFlatten { get; set; } = true;
    public bool PreferConstructor { get; set; }
    public int Strictness { get; set; }
}

public class FlattenPathModel
{
    public string FullPath { get; set; } = string.Empty;
    public List<string> Segments { get; set; } = new();
    public string TargetProperty { get; set; } = string.Empty;
}

public enum MappingStrategy
{
    Auto,
    Flatten,
    Nested,
    Manual
}
