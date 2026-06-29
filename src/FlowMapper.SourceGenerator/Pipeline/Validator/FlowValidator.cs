using System.Collections.Generic;
using FlowMapper.Core;
using FlowMapper.SourceGenerator.Models;

namespace FlowMapper.SourceGenerator.Pipeline.Validator;

public static class FlowValidator
{
    private static readonly List<IValidationRule> Rules = new()
    {
        new PropertyMatchRule(),
        new InvalidMapperRule(),
        new ConstructorRule(),
        new CycleRule(),
        new FlattenRule()
    };

    public static List<FlowDiagnosticResult> Validate(MapperDefinition candidate, Flow flow)
    {
        var diagnostics = new List<FlowDiagnosticResult>();
        foreach (var rule in Rules)
            diagnostics.AddRange(rule.Validate(candidate, flow));
        return diagnostics;
    }
}