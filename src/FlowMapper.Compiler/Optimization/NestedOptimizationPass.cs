using System.Diagnostics;
using FlowMapper.Compiler.Metadata;

namespace FlowMapper.Compiler.Optimization;

public sealed class NestedOptimizationPass : IOptimizationPass
{
    public string Name => "Nested Optimization";

    public IOptimizedMetadataModel Execute(IMetadataModel metadata)
    {
        var sw = Stopwatch.StartNew();
        var reused = 0;

        sw.Stop();
        return new OptimizedMetadataModel(
            metadata.Types,
            [new OptimizationReport(Name, "Reused existing artifacts for nested objects", reused, 0, sw.Elapsed)],
            metadata.Types.Count);
    }
}
