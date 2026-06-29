using System.Collections.Generic;
using System.Linq;
using FlowMapper.Core;
using FlowMapper.SourceGenerator.Models;

namespace FlowMapper.SourceGenerator.Pipeline.Validator;

public class CycleRule : IValidationRule
{
    public string RuleId => "Cycle";

    public IEnumerable<FlowDiagnosticResult> Validate(MapperDefinition candidate, Flow flow)
    {
        // FM0006 — Detect cycles in nested flows
        var visited = new HashSet<string>();
        var path = new List<string>();
        var results = new List<FlowDiagnosticResult>();

        foreach (var nested in flow.NestedFlows)
        {
            DetectCycle(nested.ChildFlow, visited, path, results);
        }

        return results;
    }

    private static void DetectCycle(
        Flow flow,
        HashSet<string> visited,
        List<string> path,
        List<FlowDiagnosticResult> results)
    {
        var typeKey = $"{flow.SourceType}->{flow.DestinationType}";

        if (path.Contains(typeKey))
        {
            var cyclePath = string.Join(" -> ", path.SkipWhile(p => p != typeKey).Append(typeKey));
            results.Add(FlowDiagnosticResult.Error(
                "FM0006",
                $"Cycle detected in mapping path: {cyclePath}"));
            return;
        }

        if (!visited.Add(typeKey))
            return;

        path.Add(typeKey);

        foreach (var nested in flow.NestedFlows)
        {
            DetectCycle(nested.ChildFlow, visited, path, results);
        }

        path.RemoveAt(path.Count - 1);
    }
}