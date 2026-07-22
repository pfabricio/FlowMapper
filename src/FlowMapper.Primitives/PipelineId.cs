namespace FlowMapper.Primitives;

public readonly record struct PipelineId(Guid Value)
{
    public static PipelineId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString("N");
}
