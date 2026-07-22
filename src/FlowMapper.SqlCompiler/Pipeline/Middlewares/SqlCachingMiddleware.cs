using System.Collections.Concurrent;

namespace FlowMapper.SqlCompiler.Pipeline.Middlewares;

public sealed class SqlCachingMiddleware : ISqlMiddleware
{
    private readonly ConcurrentDictionary<string, CompiledSql> _cache = new();

    public CompiledSql Process(string sql, object? parameters, SqlDelegate next)
    {
        var key = parameters != null
            ? $"{sql}:{string.Join(",", parameters.GetType().GetProperties().Select(p => p.Name))}"
            : sql;

        return _cache.GetOrAdd(key, _ => next(sql, parameters));
    }
}
