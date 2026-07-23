namespace FlowMapper.Execution.Artifacts;

public interface IMappingArtifact : IExecutionArtifact
{
    Type SourceType { get; }
    Type DestinationType { get; }
    Delegate? MappingDelegate { get; }
    Delegate? ReverseMappingDelegate { get; }
    Delegate? BeforeMapDelegate { get; }
    Delegate? AfterMapDelegate { get; }
}

public sealed record MappingArtifact(
    string Name,
    Version Version,
    Type SourceType,
    Type DestinationType,
    Delegate? MappingDelegate,
    Delegate? ReverseMappingDelegate,
    Delegate? BeforeMapDelegate,
    Delegate? AfterMapDelegate
) : IMappingArtifact;
