using System.Diagnostics;
using FlowMapper.Compiler.Metadata;

namespace FlowMapper.Compiler.Optimization;

public sealed class DelegateFusionPass : IOptimizationPass
{
    public string Name => "Delegate Fusion";

    public IOptimizedMetadataModel Execute(IMetadataModel metadata)
    {
        var sw = Stopwatch.StartNew();
        var fused = 0;

        sw.Stop();
        return new OptimizedMetadataModel(
            metadata.Types,
            [new OptimizationReport(Name, "Fused compatible delegate chains into single delegates", 0, fused, sw.Elapsed)],
            metadata.Types.Count);
    }
}
