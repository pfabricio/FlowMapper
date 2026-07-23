namespace FlowMapper.Execution.Artifacts;

public interface IExecutionArtifact
{
    string Name { get; }
    Version Version { get; }
}
