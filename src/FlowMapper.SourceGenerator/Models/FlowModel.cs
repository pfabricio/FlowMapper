using FlowMapper.SourceGenerator.Pipeline.Validator;

namespace FlowMapper.SourceGenerator.Models;

public class FlowModel
{
    public List<FlowDescriptor> Flows { get; }
    public string MapperName { get; }
    public List<FlowDiagnosticResult> Diagnostics { get; }

    public FlowModel(List<FlowDescriptor> flows, string mapperName, List<FlowDiagnosticResult> diagnostics)
    {
        Flows = flows;
        MapperName = mapperName;
        Diagnostics = diagnostics;
    }
}