using FlowMapper.Abstractions;

namespace FlowMapper.SqlCompiler.Pipeline.Middlewares;

public sealed class DialectMiddleware : ISqlMiddleware
{
    private readonly IDialect _dialect;

    public DialectMiddleware(IDialect dialect)
    {
        _dialect = dialect;
    }

    public CompiledSql Process(string sql, object? parameters, SqlDelegate next)
    {
        var result = next(sql, parameters);

        var normalizedParams = result.Parameters
            .Select(p => new Execution.Artifacts.ParameterBinding(
                p.Name,
                p.Type,
                p.Size))
            .ToList();

        var text = result.CommandText;
        text = _dialect.ApplyPagination(text, 0, 0); // no-op unless SQL has pagination markers

        return new CompiledSql(text, normalizedParams);
    }
}
