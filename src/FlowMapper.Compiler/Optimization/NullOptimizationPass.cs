using System.Diagnostics;
using FlowMapper.Compiler.Metadata;

namespace FlowMapper.Compiler.Optimization;

public sealed class NullOptimizationPass : IOptimizationPass
{
    public string Name => "Null Optimization";

    public IOptimizedMetadataModel Execute(IMetadataModel metadata)
    {
        var sw = Stopwatch.StartNew();
        var removed = 0;

        var optimized = metadata.Types
            .Select(t => RemoveRedundantNullChecks(t, ref removed))
            .ToList()
            .AsReadOnly();

        sw.Stop();
        return new OptimizedMetadataModel(
            optimized,
            [new OptimizationReport(Name, "Removed provably redundant null checks", removed, 0, sw.Elapsed)],
            metadata.Types.Count);
    }

    private static ITypeMetadata RemoveRedundantNullChecks(ITypeMetadata type, ref int removed)
    {
        return type;
    }
}
