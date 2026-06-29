using System.Collections.Generic;
using System.Linq;
using FlowMapper.Core;
using FlowMapper.SourceGenerator.Models;

namespace FlowMapper.SourceGenerator.Pipeline.Validator;

public class ConstructorRule : IValidationRule
{
    public string RuleId => "Constructor";

    public IEnumerable<FlowDiagnosticResult> Validate(MapperDefinition candidate, Flow flow)
    {
        // Skip validation when ConstructUsing is explicitly provided
        if (flow.ConstructUsingMethod != null)
            yield break;

        var destType = candidate.DestinationType;
        var constructorBindings = flow.ConstructorBindings;

        if (constructorBindings.Count == 0)
        {
            var hasParamlessCtor = destType
                .GetMembers()
                .OfType<Microsoft.CodeAnalysis.IMethodSymbol>()
                .Any(c => c.MethodKind == Microsoft.CodeAnalysis.MethodKind.Constructor
                          && c.Parameters.Length == 0
                          && c.DeclaredAccessibility == Microsoft.CodeAnalysis.Accessibility.Public);

            if (!hasParamlessCtor)
            {
                yield return FlowDiagnosticResult.Warning(
                    "FM0007",
                    $"No suitable constructor found for type '{destType.Name}'");
            }

            yield break;
        }

        // FM0008 — Missing constructor binding
        var destCtors = destType
            .GetMembers()
            .OfType<Microsoft.CodeAnalysis.IMethodSymbol>()
            .Where(c => c.MethodKind == Microsoft.CodeAnalysis.MethodKind.Constructor
                        && c.DeclaredAccessibility == Microsoft.CodeAnalysis.Accessibility.Public)
            .ToList();

        foreach (var ctor in destCtors)
        {
            foreach (var param in ctor.Parameters)
            {
                var isBound = constructorBindings.Any(b =>
                    b.ParameterName.Equals(param.Name, System.StringComparison.OrdinalIgnoreCase));

                var hasSourceProp = candidate.SourceType
                    .GetMembers()
                    .OfType<Microsoft.CodeAnalysis.IPropertySymbol>()
                    .Any(p => p.Name.Equals(param.Name, System.StringComparison.OrdinalIgnoreCase));

                if (!isBound && hasSourceProp)
                {
                    yield return FlowDiagnosticResult.Error(
                        "FM0008",
                        $"Required constructor parameter '{param.Name}' not mapped");
                }
            }
        }
    }
}