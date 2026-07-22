using System.Diagnostics;
using FlowMapper.Compiler.Metadata;

namespace FlowMapper.Compiler.Optimization;

public sealed class ConstantEvaluationPass : IOptimizationPass
{
    public string Name => "Constant Evaluation";

    public IOptimizedMetadataModel Execute(IMetadataModel metadata)
    {
        var sw = Stopwatch.StartNew();
        var evaluated = 0;

        sw.Stop();
        return new OptimizedMetadataModel(
            metadata.Types,
            [new OptimizationReport(Name, "Resolved constant expressions at compile time", evaluated, 0, sw.Elapsed)],
            metadata.Types.Count);
    }
}
