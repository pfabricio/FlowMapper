namespace FlowMapper.Execution.Artifacts;

public interface IExecutionPlan
{
    string Name { get; }
    Version Version { get; }
    IReadOnlyCollection<IExecutionArtifact> Artifacts { get; }
    Delegate? ExecutionDelegate { get; }
}

public interface IExecutionPlanRegistry
{
    IReadOnlyCollection<IExecutionPlan> Plans { get; }
}

public sealed record ExecutionPlan(
    string Name,
    Version Version,
    IReadOnlyCollection<IExecutionArtifact> Artifacts,
    Delegate? ExecutionDelegate
) : IExecutionPlan;

public sealed record ExecutionPlanRegistry(
    IReadOnlyCollection<IExecutionPlan> Plans
) : IExecutionPlanRegistry;
