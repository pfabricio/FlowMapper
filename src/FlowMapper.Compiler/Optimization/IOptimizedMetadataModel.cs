using FlowMapper.Compiler.Metadata;

namespace FlowMapper.Compiler.Optimization;

public interface IOptimizedMetadataModel : IMetadataModel
{
    IReadOnlyCollection<OptimizationReport> AppliedOptimizations { get; }
    int OriginalTypeCount { get; }
    int OptimizedTypeCount { get; }
}

public sealed record OptimizationReport(
    string PassName,
    string Description,
    int ItemsRemoved,
    int ItemsFused,
    TimeSpan Duration);
