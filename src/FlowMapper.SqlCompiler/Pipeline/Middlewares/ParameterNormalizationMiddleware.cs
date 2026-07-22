namespace FlowMapper.SqlCompiler.Pipeline.Middlewares;

public sealed class ParameterNormalizationMiddleware : ISqlMiddleware
{
    public CompiledSql Process(string sql, object? parameters, SqlDelegate next)
    {
        var result = next(sql, parameters);

        if (parameters == null || !(parameters is IDictionary<string, object>))
            return result;

        var text = result.CommandText;
        foreach (var p in result.Parameters)
        {
            text = text.Replace($"@{p.Name}", $"@{p.Name}");
        }

        return new CompiledSql(text, result.Parameters, result.ParameterDelegate);
    }
}
