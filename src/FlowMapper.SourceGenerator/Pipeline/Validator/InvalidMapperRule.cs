using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using FlowMapper.Core;
using FlowMapper.SourceGenerator.Models;

namespace FlowMapper.SourceGenerator.Pipeline.Validator;

public class InvalidMapperRule : IValidationRule
{
    public string RuleId => "InvalidMapper";

    public IEnumerable<FlowDiagnosticResult> Validate(MapperDefinition candidate, Flow flow)
    {
        var sourceProps = candidate.SourceType
            .GetMembers()
            .OfType<IPropertySymbol>()
            .ToList();

        // FM0003 — Invalid mapper (no properties mapped)
        if (flow.Properties.Count == 0 && sourceProps.Count > 0)
        {
            yield return FlowDiagnosticResult.Error(
                "FM0003",
                $"Mapper '{candidate.MapperType.Name}' is invalid or incomplete");
        }
    }
}