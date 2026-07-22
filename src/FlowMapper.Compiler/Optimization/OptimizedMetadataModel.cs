using FlowMapper.Compiler.Metadata;

namespace FlowMapper.Compiler.Optimization;

public sealed record OptimizedMetadataModel(
    IReadOnlyCollection<ITypeMetadata> Types,
    IReadOnlyCollection<OptimizationReport> AppliedOptimizations,
    int OriginalTypeCount
) : IOptimizedMetadataModel
{
    public int OptimizedTypeCount => Types.Count;
}
