using System.Data;
using FlowMapper.Execution.Artifacts;

namespace FlowMapper.Materializer.Pipeline.Middlewares;

public sealed class ConversionMiddleware : IMaterializationMiddleware
{
    public T Materialize<T>(IDataReader reader, MaterializationDelegate<T> next)
    {
        return next(reader);
    }
}
