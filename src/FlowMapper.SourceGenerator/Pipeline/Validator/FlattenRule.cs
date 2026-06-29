using System.Collections.Generic;
using System.Linq;
using FlowMapper.Core;
using FlowMapper.SourceGenerator.Models;

namespace FlowMapper.SourceGenerator.Pipeline.Validator;

public class FlattenRule : IValidationRule
{
    public string RuleId => "Flatten";

    public IEnumerable<FlowDiagnosticResult> Validate(MapperDefinition candidate, Flow flow)
    {
        var results = new List<FlowDiagnosticResult>();

        var flattenProps = flow.Properties
            .Where(p => p.Strategy == MappingStrategy.Flatten)
            .ToList();

        foreach (var prop in flattenProps)
        {
            if (string.IsNullOrEmpty(prop.SourcePath))
            {
                results.Add(FlowDiagnosticResult.Warning(
                    "FM0010",
                    $"No valid path found for '{prop.DestinationProperty}'"));
                continue;
            }

            // FM0009 — Ambiguous path (when multiple paths exist)
            var pathSegments = prop.SourcePath.Split('.');
            if (pathSegments.Length > 3)
            {
                results.Add(FlowDiagnosticResult.Error(
                    "FM0009",
                    $"Multiple paths found for property '{prop.DestinationProperty}'"));
            }

            // FM0011 — Invalid depth / cycle-like patterns
            if (pathSegments.Length > 5)
            {
                results.Add(FlowDiagnosticResult.Error(
                    "FM0011",
                    $"Cycle or invalid depth detected in flatten graph"));
            }
        }

        return results;
    }
}