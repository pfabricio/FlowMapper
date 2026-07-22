using System.Diagnostics;
using FlowMapper.Execution;

namespace FlowMapper.Compiler.Pipeline;

public sealed class ExecutionPlanStage : ICompilerStage
{
    public string Name => "Execution Plan";

    public CompilerStageResult Execute(CompilerContext context)
    {
        var sw = Stopwatch.StartNew();
        var diagnostics = new List<CompilerDiagnostic>();

        try
        {
            var plans = new List<ExecutionPlan>();

            foreach (var artifact in context.Artifacts)
            {
                var plan = new ExecutionPlan
                {
                    Nodes = GeneratePlanNodes(artifact)
                };
                plans.Add(plan);
            }

            context.ExecutionPlans.Clear();
            context.ExecutionPlans.AddRange(plans);

            sw.Stop();
            diagnostics.Add(new CompilerDiagnostic(
                Name, $"Built {plans.Count} execution plans in {sw.Elapsed.TotalMilliseconds:F1}ms",
                CompilerDiagnosticSeverity.Info));
            return new CompilerStageResult(true, diagnostics.AsReadOnly(), sw.Elapsed);
        }
        catch (Exception ex)
        {
            sw.Stop();
            diagnostics.Add(new CompilerDiagnostic(
                Name, $"Execution plan building failed: {ex.Message}",
                CompilerDiagnosticSeverity.Error, ex.StackTrace));
            return new CompilerStageResult(false, diagnostics.AsReadOnly(), sw.Elapsed);
        }
    }

    private static List<ExecutionNode> GeneratePlanNodes(ExecutionArtifact artifact)
    {
        return artifact.Plan?.Nodes?.ToList() ?? [];
    }
}
