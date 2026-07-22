namespace FlowMapper.Primitives;

public readonly record struct ExecutionId(Guid Value)
{
    public static ExecutionId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString("N");
}
