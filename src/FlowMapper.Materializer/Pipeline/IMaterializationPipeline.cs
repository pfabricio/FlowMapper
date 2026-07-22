using System.Data;
using FlowMapper.Execution.Artifacts;

namespace FlowMapper.Materializer.Pipeline;

public interface IMaterializationPipeline
{
    T Materialize<T>(IDataReader reader, IMaterializationArtifact artifact);
    IEnumerable<T> MaterializeAll<T>(IDataReader reader, IMaterializationArtifact artifact);
}
