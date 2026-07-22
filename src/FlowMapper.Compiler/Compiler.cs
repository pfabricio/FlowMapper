using FlowMapper.Abstractions;
using FlowMapper.Core;
using FlowMapper.Execution;
using FlowMapper.Compiler.Metadata;
using FlowMapper.Compiler.Optimization;

namespace FlowMapper.Compiler;

public class Compiler : ICompiler
{
    private readonly FlowBuilder _flowBuilder;
    private readonly MetadataBuilder _metadataBuilder;
    private readonly IOptimizationEngine? _optimizationEngine;

    public Compiler(
        FlowBuilder flowBuilder,
        MetadataBuilder? metadataBuilder = null,
        IOptimizationEngine? optimizationEngine = null)
    {
        _flowBuilder = flowBuilder;
        _metadataBuilder = metadataBuilder ?? new MetadataBuilder();
        _optimizationEngine = optimizationEngine;
    }

    public IReadOnlyList<ExecutionArtifact> Compile(IReadOnlyList<ProfileDefinition> profiles)
    {
        if (_optimizationEngine != null)
        {
            var types = profiles
                .SelectMany(p => p.Registrations)
                .SelectMany(r => new[] { r.SourceType, r.DestinationType })
                .Distinct()
                .ToList();

            var metadata = _metadataBuilder.Build(types);
            var optimized = _optimizationEngine.Optimize(metadata);
        }

        var artifacts = new List<ExecutionArtifact>();

        foreach (var profile in profiles)
        {
            foreach (var registration in profile.Registrations)
            {
                if (registration.SourceType.IsGenericType &&
                    registration.SourceType.GetGenericTypeDefinition() == typeof(DataReaderMapping<>))
                {
                    artifacts.Add(new ExecutionArtifact
                    {
                        Name = $"DataReaderTo{registration.DestinationType.Name}",
                        SourceType = registration.SourceType,
                        DestinationType = registration.DestinationType,
                        Plan = new ExecutionPlan()
                    });
                    continue;
                }

                var flow = _flowBuilder.Build(registration.SourceType, registration.DestinationType, profile);
                artifacts.Add(CreateArtifact(flow));

                if (IsReverseMapped(registration))
                {
                    var reverseFlow = _flowBuilder.Build(registration.DestinationType, registration.SourceType);
                    reverseFlow.IsReverse = true;
                    artifacts.Add(CreateArtifact(reverseFlow));
                }
            }
        }

        return artifacts;
    }

    private static ExecutionArtifact CreateArtifact(Flow flow)
    {
        var artifact = new ExecutionArtifact
        {
            Name = flow.Name,
            SourceType = flow.Signature.SourceType,
            DestinationType = flow.Signature.DestinationType,
            Plan = new ExecutionPlan
            {
                Nodes = flow.Properties.Select(p => new ExecutionNode
                {
                    Name = $"{p.SourceProperty} → {p.DestinationProperty}",
                    Type = NodeType.Transformation,
                    Metadata = new Dictionary<string, object?>
                    {
                        ["SourceProperty"] = p.SourceProperty,
                        ["DestinationProperty"] = p.DestinationProperty,
                        ["IsPathMapping"] = p.IsPathMapping,
                        ["PathSegments"] = string.Join(",", p.PathSegments),
                        ["MapFromExpression"] = p.MapFromExpression
                    }
                }).Concat(flow.NestedFlows.Select(n => new ExecutionNode
                {
                    Name = $"Nested: {n.ParentProperty}",
                    Type = NodeType.Transformation,
                    Metadata = new Dictionary<string, object?> { ["ParentProperty"] = n.ParentProperty }
                })).ToList()
            }
        };
        return artifact;
    }

    private static bool IsReverseMapped(MappingRegistration registration)
    {
        var expr = registration.Expression;
        if (expr == null) return false;
        var prop = expr.GetType().GetProperty("ReverseMapped", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
        return prop != null && (bool)prop.GetValue(expr)!;
    }
}
