namespace FlowMapper.Core;

public class FlowSignature
{
    public Type SourceType { get; init; } = null!;
    public Type DestinationType { get; init; } = null!;

    public override bool Equals(object? obj) =>
        obj is FlowSignature other &&
        SourceType == other.SourceType &&
        DestinationType == other.DestinationType;

    public override int GetHashCode() =>
        HashCode.Combine(SourceType, DestinationType);

    public override string ToString() =>
        $"{SourceType.Name} → {DestinationType.Name}";
}
