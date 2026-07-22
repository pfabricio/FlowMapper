using System.Data;
using FlowMapper.Execution.Artifacts;
using FlowMapper.Execution;
using FlowMapper.Materializer.Pipeline;

namespace FlowMapper.Materializer;

public interface IMaterializer
{
    T Materialize<T>(IDataReader reader, MaterializationPlan plan);
    IEnumerable<T> MaterializeAll<T>(IDataReader reader, MaterializationPlan plan);

    T Materialize<T>(IDataReader reader, IMaterializationArtifact artifact);
    IEnumerable<T> MaterializeAll<T>(IDataReader reader, IMaterializationArtifact artifact);

    MaterializationPlan BuildPlan<T>();
}
