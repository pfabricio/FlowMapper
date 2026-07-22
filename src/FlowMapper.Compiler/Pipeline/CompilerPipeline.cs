using System.Diagnostics;

namespace FlowMapper.Compiler.Pipeline;

public sealed class CompilerPipeline
{
    private readonly IReadOnlyList<ICompilerStage> _stages;

    public CompilerPipeline(IReadOnlyList<ICompilerStage> stages)
    {
        _stages = stages;
    }

    public CompilerPipelineResult Execute(CompilerContext context)
    {
        var startedAt = DateTime.UtcNow;
        var sw = Stopwatch.StartNew();
        var stageMetrics = new Dictionary<string, StageMetrics>();
        var allDiagnostics = new List<CompilerDiagnostic>();
        var successful = 0;
        var failed = 0;

        foreach (var stage in _stages)
        {
            var stageSw = Stopwatch.StartNew();

            if (context.HasCriticalErrors)
            {
                stageSw.Stop();
                stageMetrics[stage.Name] = new StageMetrics(stage.Name, TimeSpan.Zero, false, 0);
                failed++;
                continue;
            }

            try
            {
                var result = stage.Execute(context);
                stageSw.Stop();

                var stageResult = new StageMetrics(
                    stage.Name,
                    stageSw.Elapsed,
                    result.Success,
                    result.Diagnostics.Count);

                stageMetrics[stage.Name] = stageResult;
                allDiagnostics.AddRange(result.Diagnostics);

                if (result.Success)
                    successful++;
                else
                    failed++;
            }
            catch (Exception ex)
            {
                stageSw.Stop();
                stageMetrics[stage.Name] = new StageMetrics(stage.Name, stageSw.Elapsed, false, 0);
                allDiagnostics.Add(new CompilerDiagnostic(
                    stage.Name,
                    $"Stage threw exception: {ex.Message}",
                    CompilerDiagnosticSeverity.Error,
                    ex.StackTrace));
                failed++;
            }
        }

        sw.Stop();

        var stats = new PipelineStatistics
        {
            StartedAt = startedAt,
            TotalDuration = sw.Elapsed,
            TotalStages = _stages.Count,
            SuccessfulStages = successful,
            FailedStages = failed,
            StageMetrics = stageMetrics
        };

        return new CompilerPipelineResult(
            failed == 0,
            context.Artifacts.AsReadOnly(),
            context.ExecutionPlans.AsReadOnly(),
            allDiagnostics.AsReadOnly(),
            stats,
            []);
    }
}
