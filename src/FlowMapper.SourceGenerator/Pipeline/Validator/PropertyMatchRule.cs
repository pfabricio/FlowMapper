using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using FlowMapper.Core;
using FlowMapper.SourceGenerator.Models;

namespace FlowMapper.SourceGenerator.Pipeline.Validator;

public class PropertyMatchRule : IValidationRule
{
    public string RuleId => "PropertyMatch";

    public IEnumerable<FlowDiagnosticResult> Validate(MapperDefinition candidate, Flow flow)
    {
        var diagnostics = new List<FlowDiagnosticResult>();

        var sourceProps = candidate.SourceType
            .GetMembers()
            .OfType<IPropertySymbol>()
            .ToList();

        var destProps = candidate.DestinationType
            .GetMembers()
            .OfType<IPropertySymbol>()
            .ToList();

        var mappedDestinations = new HashSet<string>(
            flow.Properties.Select(p => p.DestinationProperty)
                .Concat(flow.NestedFlows.Select(n => n.ParentProperty)));

        var mappedSources = new HashSet<string>(
            flow.Properties.Select(p => p.SourceProperty));

        // FM0004 — Source property without matching destination
        foreach (var sp in sourceProps)
        {
            if (!mappedSources.Contains(sp.Name))
            {
                diagnostics.Add(FlowDiagnosticResult.Warning(
                    "FM0004",
                    $"Source property '{sp.Name}' has no matching destination"));
            }
        }

        // FM0001 — Destination property without matching source
        // FM0002 — Same name but type mismatch
        foreach (var dp in destProps)
        {
            if (mappedDestinations.Contains(dp.Name))
                continue;

            var sourceMatch = sourceProps.FirstOrDefault(s => s.Name == dp.Name);
            if (sourceMatch != null)
            {
                if (!SymbolEqualityComparer.Default.Equals(sourceMatch.Type, dp.Type))
                {
                    diagnostics.Add(FlowDiagnosticResult.Error(
                        "FM0002",
                        $"Cannot map '{sourceMatch.Type.Name}' to '{dp.Type.Name}' for property '{dp.Name}'"));
                }
            }
            else
            {
                diagnostics.Add(FlowDiagnosticResult.Warning(
                    "FM0001",
                    $"Destination property '{dp.Name}' is not mapped"));
            }
        }

        return diagnostics;
    }
}