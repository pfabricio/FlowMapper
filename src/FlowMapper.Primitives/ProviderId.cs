namespace FlowMapper.Primitives;

public readonly record struct ProviderId(string Name)
{
    public override string ToString() => Name;
}
