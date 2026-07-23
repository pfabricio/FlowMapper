using Microsoft.CodeAnalysis;
using FlowMapper.SourceGenerator.Models;

namespace FlowMapper.SourceGenerator.Pipeline.Validator;

public class InvalidMapperRule : IValidationRule
{
    public string RuleId => "InvalidMapper";

    public IEnumerable<FlowDiagnosticResult> Validate(MapperDefinition candidate, FlowDescriptor flow)
    {
        var sourceProps = candidate.SourceType
            .GetMembers()
            .OfType<IPropertySymbol>()
            .ToList();

        if (flow.Properties.Count == 0 && sourceProps.Count > 0)
        {
            yield return FlowDiagnosticResult.Error(
                "FM0003",
                $"Mapper '{candidate.MapperType.Name}' is invalid or incomplete");
        }
    }
}