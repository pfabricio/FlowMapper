namespace FlowMapper.Execution.Artifacts;

public interface IProviderArtifact : IExecutionArtifact
{
    string ProviderName { get; }
    Version ProviderVersion { get; }
    string Category { get; }
}

public sealed record ProviderArtifact(
    string Name,
    Version Version,
    string ProviderName,
    Version ProviderVersion,
    string Category
) : IProviderArtifact;
