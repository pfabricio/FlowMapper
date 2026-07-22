using System.Data;
using FlowMapper.Execution.Artifacts;

namespace FlowMapper.Materializer.Pipeline;

public sealed class MaterializationPipeline : IMaterializationPipeline
{
    private readonly IReadOnlyList<IMaterializationMiddleware> _middlewares;
    private readonly MaterializationDelegateBuilder _builder;

    public MaterializationPipeline(
        IEnumerable<IMaterializationMiddleware>? middlewares = null,
        MaterializationDelegateBuilder? builder = null,
        string? separator = "_")
    {
        _middlewares = (middlewares as IReadOnlyList<IMaterializationMiddleware> ?? middlewares?.ToList()) ?? [];
        _builder = builder ?? new MaterializationDelegateBuilder(separator);
    }

    public T Materialize<T>(IDataReader reader, IMaterializationArtifact artifact)
    {
        var coreDelegate = _builder.BuildDelegate<T>(artifact);
        var pipeline = BuildPipeline(coreDelegate);
        return pipeline(reader);
    }

    public IEnumerable<T> MaterializeAll<T>(IDataReader reader, IMaterializationArtifact artifact)
    {
        while (reader.Read())
        {
            yield return Materialize<T>(reader, artifact);
        }
    }

    private MaterializationDelegate<T> BuildPipeline<T>(MaterializationDelegate<T> core)
    {
        MaterializationDelegate<T> pipeline = core;
        for (var i = _middlewares.Count - 1; i >= 0; i--)
        {
            var middleware = _middlewares[i];
            var current = pipeline;
            pipeline = reader => middleware.Materialize(reader, current);
        }
        return pipeline;
    }
}
