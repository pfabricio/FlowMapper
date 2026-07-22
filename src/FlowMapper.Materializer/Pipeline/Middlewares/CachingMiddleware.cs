using System.Data;
using FlowMapper.Execution.Artifacts;

namespace FlowMapper.Materializer.Pipeline.Middlewares;

public sealed class CachingMiddleware : IMaterializationMiddleware
{
    private readonly Dictionary<string, Delegate> _cache = new();

    public T Materialize<T>(IDataReader reader, MaterializationDelegate<T> next)
    {
        return next(reader);
    }
}
