namespace FlowMapper.Abstractions;

public class FlowMapperOptions
{
    public DataOptions Data { get; init; } = new();
    public MappingOptionsSection Mapping { get; init; } = new();
}

public class DataOptions
{
    public int? DefaultTimeout { get; set; }
    public string CascadeSeparator { get; set; } = "_";
    public MappingOptions Mapping { get; set; } = new();
    public RetryOptions Retry { get; set; } = new();
}

public class RetryOptions
{
    public bool Enabled { get; set; }
    public int MaxRetries { get; set; } = 3;
    public int InitialDelayMs { get; set; } = 100;
}

public class MappingOptionsSection
{
    public string DefaultProfile { get; set; } = "Default";
    public bool EnableFlatten { get; set; } = true;
    public bool PreferConstructorMapping { get; set; }
    public StrictnessLevel Strictness { get; set; } = StrictnessLevel.None;
    public bool EnableCache { get; set; } = true;
}
