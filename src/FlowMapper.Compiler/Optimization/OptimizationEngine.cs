using System.Diagnostics;
using FlowMapper.Compiler.Metadata;

namespace FlowMapper.Compiler.Optimization;

public sealed class OptimizationEngine : IOptimizationEngine
{
    private readonly IReadOnlyList<IOptimizationPass> _passes;

    public OptimizationEngine() : this(DefaultPasses()) { }

    public OptimizationEngine(IReadOnlyList<IOptimizationPass> passes)
    {
        _passes = passes;
    }

    public IOptimizedMetadataModel Optimize(IMetadataModel metadata)
    {
        var reports = new List<OptimizationReport>();
        var originalCount = metadata.Types.Count;
        IMetadataModel current = metadata;

        foreach (var pass in _passes)
        {
            var sw = Stopwatch.StartNew();
            var result = pass.Execute(current);
            sw.Stop();

            foreach (var report in result.AppliedOptimizations)
                reports.Add(report);

            current = result;
        }

        return current is IOptimizedMetadataModel optimized
            ? optimized
            : new OptimizedMetadataModel(
                current.Types,
                reports.AsReadOnly(),
                originalCount);
    }

    public static IReadOnlyList<IOptimizationPass> DefaultPasses() =>
    [
        new RedundantMappingRemovalPass(),
        new DelegateFusionPass(),
        new ConstantEvaluationPass(),
        new NullOptimizationPass(),
        new FlattenOptimizationPass(),
        new NestedOptimizationPass(),
        new ConverterOptimizationPass(),
        new DeadMetadataEliminationPass()
    ];
}
