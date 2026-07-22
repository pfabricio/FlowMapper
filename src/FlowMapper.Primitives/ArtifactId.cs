namespace FlowMapper.Primitives;

public readonly record struct ArtifactId(Guid Value)
{
    public static ArtifactId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString("N");
}
