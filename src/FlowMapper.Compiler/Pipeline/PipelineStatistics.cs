using System.Collections.Concurrent;

namespace FlowMapper.Compiler.Pipeline;

public sealed record PipelineStatistics
{
    public DateTime StartedAt { get; init; }
    public TimeSpan TotalDuration { get; init; }
    public int TotalStages { get; init; }
    public int SuccessfulStages { get; init; }
    public int FailedStages { get; init; }
    public IReadOnlyDictionary<string, StageMetrics> StageMetrics { get; init; }
        = new Dictionary<string, StageMetrics>();

    public static PipelineStatistics Empty { get; } = new();
}

public sealed record StageMetrics(
    string StageName,
    TimeSpan Duration,
    bool Success,
    int DiagnosticCount);
