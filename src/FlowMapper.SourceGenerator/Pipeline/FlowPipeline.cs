using FlowMapper.Core;
using FlowMapper.SourceGenerator.Models;
using FlowMapper.SourceGenerator.Performance;
using FlowMapper.SourceGenerator.Pipeline.Builder;
using FlowMapper.SourceGenerator.Pipeline.Validator;

namespace FlowMapper.SourceGenerator.Pipeline;

public static class FlowPipeline
{
    public static FlowModel Execute(IReadOnlyList<MapperDefinition> definitions)
    {
        var cache = new FlowCache();
        var flows = new List<Flow>();
        var diagnostics = new List<FlowDiagnosticResult>();

        foreach (var definition in definitions)
        {
            var flow = Build(definition, cache);
            flows.Add(flow);
            diagnostics.AddRange(Validate(definition, flow));
        }

        var mapperName = ResolveMapperName(definitions);
        return new FlowModel(flows, mapperName, diagnostics);
    }

    public static Flow Build(MapperDefinition definition, FlowCache? cache = null)
    {
        return FlowBuilder.Build(definition, cache);
    }

    public static List<FlowDiagnosticResult> Validate(MapperDefinition definition, Flow flow)
    {
        return FlowValidator.Validate(definition, flow);
    }

    public static string ResolveMapperName(IReadOnlyList<MapperDefinition> definitions)
    {
        var mapperName = string.Empty;
        foreach (var def in definitions)
        {
            var name = def.MapperName ?? def.MapperType.Name;
            if (string.IsNullOrEmpty(mapperName))
                mapperName = name;
            else if (mapperName != name)
                mapperName = "AggregateMapper";
        }
        return mapperName;
    }
}