using FlowMapper.Execution;

namespace FlowMapper.Compiler.Pipeline;

public sealed record CompilerPipelineResult(
    bool Success,
    IReadOnlyList<ExecutionArtifact> Artifacts,
    IReadOnlyList<ExecutionPlan> ExecutionPlans,
    IReadOnlyList<CompilerDiagnostic> Diagnostics,
    PipelineStatistics Statistics,
    IReadOnlyList<string> GeneratedSources);
