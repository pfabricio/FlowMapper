using System.Diagnostics;
using FlowMapper.Compiler.Optimization;

namespace FlowMapper.Compiler.Pipeline;

public sealed class OptimizationStage : ICompilerStage
{
    private readonly IOptimizationEngine _engine;

    public OptimizationStage(IOptimizationEngine? engine = null)
    {
        _engine = engine ?? new OptimizationEngine();
    }

    public string Name => "Optimization";

    public CompilerStageResult Execute(CompilerContext context)
    {
        var sw = Stopwatch.StartNew();
        var diagnostics = new List<CompilerDiagnostic>();

        if (context.Metadata == null)
        {
            sw.Stop();
            diagnostics.Add(new CompilerDiagnostic(
                Name, "Optimization skipped (no metadata available)",
                CompilerDiagnosticSeverity.Warning));
            return new CompilerStageResult(true, diagnostics.AsReadOnly(), sw.Elapsed);
        }

        try
        {
            var optimized = _engine.Optimize(context.Metadata);
            context.OptimizedMetadata = optimized;

            foreach (var report in optimized.AppliedOptimizations)
            {
                diagnostics.Add(new CompilerDiagnostic(
                    Name,
                    $"{report.PassName}: removed {report.ItemsRemoved}, fused {report.ItemsFused} in {report.Duration.TotalMilliseconds:F1}ms",
                    CompilerDiagnosticSeverity.Info));
            }

            sw.Stop();
            diagnostics.Add(new CompilerDiagnostic(
                Name,
                $"Optimized {optimized.OriginalTypeCount} → {optimized.OptimizedTypeCount} types with {optimized.AppliedOptimizations.Count} passes",
                CompilerDiagnosticSeverity.Info));
            return new CompilerStageResult(true, diagnostics.AsReadOnly(), sw.Elapsed);
        }
        catch (Exception ex)
        {
            sw.Stop();
            diagnostics.Add(new CompilerDiagnostic(
                Name, $"Optimization failed: {ex.Message}",
                CompilerDiagnosticSeverity.Error, ex.StackTrace));
            return new CompilerStageResult(false, diagnostics.AsReadOnly(), sw.Elapsed);
        }
    }
}
