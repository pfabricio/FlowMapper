using System.Data;
using FlowMapper.Execution.Artifacts;

namespace FlowMapper.Materializer.Pipeline;

public delegate T MaterializationDelegate<T>(IDataReader reader);

public interface IMaterializationMiddleware
{
    T Materialize<T>(IDataReader reader, MaterializationDelegate<T> next);
}
