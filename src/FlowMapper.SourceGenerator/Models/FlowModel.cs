using FlowMapper.Core;
using FlowMapper.SourceGenerator.Pipeline.Validator;

namespace FlowMapper.SourceGenerator.Models;

public class FlowModel
{
    public List<Flow> Flows { get; }
    public string MapperName { get; }
    public List<FlowDiagnosticResult> Diagnostics { get; }

    public FlowModel(List<Flow> flows, string mapperName, List<FlowDiagnosticResult> diagnostics)
    {
        Flows = flows;
        MapperName = mapperName;
        Diagnostics = diagnostics;
    }
}
