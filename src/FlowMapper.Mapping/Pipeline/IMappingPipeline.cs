using FlowMapper.Execution.Artifacts;

namespace FlowMapper.Mapping.Pipeline;

public interface IMappingPipeline
{
    TDestination Map<TSource, TDestination>(TSource source, IMappingArtifact artifact);
    IReadOnlyList<TDestination> MapAll<TSource, TDestination>(
        IEnumerable<TSource> source, IMappingArtifact artifact);
}
