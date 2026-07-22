using System.Diagnostics;
using FlowMapper.Compiler.Metadata;

namespace FlowMapper.Compiler.Optimization;

public sealed class FlattenOptimizationPass : IOptimizationPass
{
    public string Name => "Flatten Optimization";

    public IOptimizedMetadataModel Execute(IMetadataModel metadata)
    {
        var sw = Stopwatch.StartNew();
        var flattened = 0;

        sw.Stop();
        return new OptimizedMetadataModel(
            metadata.Types,
            [new OptimizationReport(Name, "Optimized property chain access paths", flattened, 0, sw.Elapsed)],
            metadata.Types.Count);
    }
}
