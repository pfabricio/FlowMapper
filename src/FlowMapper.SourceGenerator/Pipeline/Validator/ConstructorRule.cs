using Microsoft.CodeAnalysis;
using FlowMapper.SourceGenerator.Models;

namespace FlowMapper.SourceGenerator.Pipeline.Validator;

public class ConstructorRule : IValidationRule
{
    public string RuleId => "Constructor";

    public IEnumerable<FlowDiagnosticResult> Validate(MapperDefinition candidate, FlowDescriptor flow)
    {
        if (flow.ConstructUsingMethod != null)
            yield break;

        var destType = candidate.DestinationType;
        var constructorBindings = flow.ConstructorBindings;

        if (constructorBindings.Count == 0)
        {
            var hasParamlessCtor = destType
                .GetMembers()
                .OfType<IMethodSymbol>()
                .Any(c => c.MethodKind == MethodKind.Constructor
                          && c.Parameters.Length == 0
                          && c.DeclaredAccessibility == Accessibility.Public);

            if (!hasParamlessCtor)
            {
                yield return FlowDiagnosticResult.Warning(
                    "FM0007",
                    $"No suitable constructor found for type '{destType.Name}'");
            }

            yield break;
        }

        var destCtors = destType
            .GetMembers()
            .OfType<IMethodSymbol>()
            .Where(c => c.MethodKind == MethodKind.Constructor
                        && c.DeclaredAccessibility == Accessibility.Public)
            .ToList();

        foreach (var ctor in destCtors)
        {
            foreach (var param in ctor.Parameters)
            {
                var isBound = constructorBindings.Any(b =>
                    b.ParameterName.Equals(param.Name, StringComparison.OrdinalIgnoreCase));

                var hasSourceProp = candidate.SourceType
                    .GetMembers()
                    .OfType<IPropertySymbol>()
                    .Any(p => p.Name.Equals(param.Name, StringComparison.OrdinalIgnoreCase));

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