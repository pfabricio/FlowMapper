using FlowMapper.Execution.Artifacts;

namespace FlowMapper.SqlCompiler.Pipeline;

public sealed class SqlPipeline : ISqlCompiler
{
    private readonly IReadOnlyList<ISqlMiddleware> _middlewares;
    private readonly SqlDelegateBuilder _builder;

    public SqlPipeline(
        IEnumerable<ISqlMiddleware>? middlewares = null,
        SqlDelegateBuilder? builder = null)
    {
        _middlewares = (middlewares as IReadOnlyList<ISqlMiddleware> ?? middlewares?.ToList()) ?? [];
        _builder = builder ?? new SqlDelegateBuilder();
    }

    public CompiledSql Compile(ISqlArtifact artifact)
    {
        return new CompiledSql(
            artifact.CommandText,
            artifact.Parameters.ToList(),
            artifact.ExecutionDelegate);
    }

    public CompiledSql Compile(string sql, object? parameters = null)
    {
        var coreDelegate = _builder.BuildDelegate();
        var pipeline = BuildPipeline(coreDelegate);
        return pipeline(sql, parameters);
    }

    private SqlDelegate BuildPipeline(SqlDelegate core)
    {
        SqlDelegate pipeline = core;
        for (var i = _middlewares.Count - 1; i >= 0; i--)
        {
            var middleware = _middlewares[i];
            var current = pipeline;
            pipeline = (sql, p) => middleware.Process(sql, p, current);
        }
        return pipeline;
    }
}
