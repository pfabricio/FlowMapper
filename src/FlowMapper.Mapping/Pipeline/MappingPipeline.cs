using FlowMapper.Core;
using FlowMapper.Execution.Artifacts;

namespace FlowMapper.Mapping.Pipeline;

public sealed class MappingPipeline : IMappingPipeline
{
    private readonly IReadOnlyList<IMappingMiddleware> _middlewares;
    private readonly MappingDelegateBuilder _builder;

    public MappingPipeline(
        IEnumerable<IMappingMiddleware>? middlewares = null,
        MappingDelegateBuilder? builder = null)
    {
        _middlewares = (middlewares as IReadOnlyList<IMappingMiddleware> ?? middlewares?.ToList()) ?? [];
        _builder = builder ?? new MappingDelegateBuilder();
    }

    public TDestination Map<TSource, TDestination>(
        TSource source, IMappingArtifact artifact)
    {
        var coreDelegate = _builder.BuildDelegate<TSource, TDestination>(artifact);
        var pipeline = BuildPipeline(coreDelegate);
        return pipeline(source);
    }

    public IReadOnlyList<TDestination> MapAll<TSource, TDestination>(
        IEnumerable<TSource> source, IMappingArtifact artifact)
    {
        var coreDelegate = _builder.BuildDelegate<TSource, TDestination>(artifact);
        var pipeline = BuildPipeline(coreDelegate);
        return source.Select(s => pipeline(s)).ToList();
    }

    private MappingDelegate<TSource, TDestination> BuildPipeline<TSource, TDestination>(
        MappingDelegate<TSource, TDestination> core)
    {
        MappingDelegate<TSource, TDestination> pipeline = core;
        for (var i = _middlewares.Count - 1; i >= 0; i--)
        {
            var middleware = _middlewares[i];
            var current = pipeline;
            pipeline = source => middleware.Map(source, current);
        }
        return pipeline;
    }
}
