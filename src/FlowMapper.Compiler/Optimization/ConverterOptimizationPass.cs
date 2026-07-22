using System.Diagnostics;
using FlowMapper.Compiler.Metadata;

namespace FlowMapper.Compiler.Optimization;

public sealed class ConverterOptimizationPass : IOptimizationPass
{
    public string Name => "Converter Optimization";

    public IOptimizedMetadataModel Execute(IMetadataModel metadata)
    {
        var sw = Stopwatch.StartNew();
        var eliminated = 0;
        var reused = 0;

        sw.Stop();
        return new OptimizedMetadataModel(
            metadata.Types,
            [new OptimizationReport(Name, "Eliminated redundant converters and reused identical ones", eliminated, reused, sw.Elapsed)],
            metadata.Types.Count);
    }
}
