using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using FlowMapper.Core;
using FlowMapper.SourceGenerator.Models;
using FlowMapper.SourceGenerator.Performance;

namespace FlowMapper.SourceGenerator.Pipeline.Builder;

public static class FlowBuilder
{
    public static Flow Build(MapperDefinition candidate, FlowCache? cache = null)
    {
        var flow = Build(candidate.SourceType, candidate.DestinationType, new HashSet<string>(), cache);
        flow.ProfileName = candidate.ProfileName;

        if (candidate.ProfilePolicy != null)
        {
            flow.Policy.EnableFlatten = candidate.ProfilePolicy.EnableFlatten;
            flow.Policy.PreferConstructor = candidate.ProfilePolicy.PreferConstructor;
            flow.Policy.Strictness = candidate.ProfilePolicy.Strictness;
        }

        flow.AfterMapMethod = candidate.AfterMapMethod;
        flow.ConstructUsingMethod = candidate.ConstructUsingMethod;

        ApplyExplicitMappings(flow, candidate);

        return flow;
    }

    private static void ApplyExplicitMappings(Flow flow, MapperDefinition candidate)
    {
        if (candidate.IgnoredProperties.Count > 0 || candidate.ExplicitMappings.Count > 0)
        {
            flow.Properties.RemoveAll(p =>
                candidate.IgnoredProperties.Contains(p.DestinationProperty));
        }

        foreach (var explicitMapping in candidate.ExplicitMappings)
        {
            if (explicitMapping.MapFromExpression != null)
            {
                var existing = flow.Properties
                    .FirstOrDefault(p => p.DestinationProperty == explicitMapping.DestinationProperty);
                if (existing != null)
                {
                    existing.MapFromExpression = explicitMapping.MapFromExpression;
                }
                else
                {
                    flow.Properties.Add(new PropertyFlow
                    {
                        SourceProperty = explicitMapping.SourceProperty,
                        DestinationProperty = explicitMapping.DestinationProperty,
                        MapFromExpression = explicitMapping.MapFromExpression,
                        Strategy = MappingStrategy.Direct
                    });
                }
                continue;
            }

            var existingProp = flow.Properties
                .FirstOrDefault(p => p.DestinationProperty == explicitMapping.DestinationProperty);
            if (existingProp != null)
            {
                if (existingProp.Strategy is MappingStrategy.Direct or MappingStrategy.Flatten)
                {
                    existingProp.SourceProperty = explicitMapping.SourceProperty;
                    existingProp.SourcePath = explicitMapping.SourceProperty;
                }
            }
            else
            {
                flow.Properties.Add(new PropertyFlow
                {
                    SourceProperty = explicitMapping.SourceProperty,
                    DestinationProperty = explicitMapping.DestinationProperty,
                    SourcePath = explicitMapping.SourceProperty,
                    Strategy = MappingStrategy.Direct
                });
            }
        }
    }

    public static Flow Build(
        INamedTypeSymbol sourceType,
        INamedTypeSymbol destType,
        HashSet<string> visited,
        FlowCache? cache = null)
    {
        if (cache != null)
        {
            var cacheKey = FlowKeyGenerator.Create(sourceType, destType);
            if (cache.TryGet(cacheKey, out var cachedFlow))
                return cachedFlow!;
        }

        var flow = BuildCore(sourceType, destType, visited, cache);

        if (cache != null)
        {
            var cacheKey = FlowKeyGenerator.Create(sourceType, destType);
            cache.Set(cacheKey, flow);
        }

        return flow;
    }

    private static Flow BuildCore(
        INamedTypeSymbol sourceType,
        INamedTypeSymbol destType,
        HashSet<string> visited,
        FlowCache? cache = null)
    {
        var flow = new Flow
        {
            SourceType = sourceType.Name,
            DestinationType = destType.Name,
            Policy = new MappingPolicy()
        };

        var sourceProps = sourceType
            .GetMembers()
            .OfType<IPropertySymbol>()
            .ToList();

        var destProps = destType
            .GetMembers()
            .OfType<IPropertySymbol>()
            .ToDictionary(x => x.Name);

        var matched = new List<(IPropertySymbol Source, IPropertySymbol Dest)>();

        foreach (var sp in sourceProps)
        {
            if (!destProps.TryGetValue(sp.Name, out var dp))
                continue;

            if (SymbolEqualityComparer.Default.Equals(sp.Type, dp.Type))
            {
                matched.Add((sp, dp));
            }
            else if (IsComplexType(sp.Type) && IsComplexType(dp.Type)
                     && sp.Type is INamedTypeSymbol srcNested
                     && dp.Type is INamedTypeSymbol dstNested)
            {
                var key = $"{srcNested.Name}->{dstNested.Name}";
                if (!visited.Contains(key))
                {
                    visited.Add(key);
                    var childFlow = Build(srcNested, dstNested, visited, cache);
                    if (childFlow.Properties.Count > 0 || childFlow.NestedFlows.Count > 0)
                    {
                        flow.NestedFlows.Add(new NestedFlow
                        {
                            ParentProperty = sp.Name,
                            ChildFlow = childFlow,
                            Strategy = MappingStrategy.Nested
                        });
                    }
                }
            }
        }

        var usedDestNames = new HashSet<string>();

        if (flow.ConstructUsingMethod == null)
        {
            var bindings = ConstructorResolver.Resolve(sourceType, destType, matched.Select(m => m.Dest.Name).ToList());
            if (bindings != null)
            {
                foreach (var binding in bindings)
                {
                    flow.ConstructorBindings.Add(binding);
                    usedDestNames.Add(binding.ParameterName);

                    flow.Properties.Add(new PropertyFlow
                    {
                        SourceProperty = binding.SourceProperty,
                        DestinationProperty = binding.ParameterName,
                        Strategy = MappingStrategy.Constructor,
                        ConstructorParameterIndex = binding.Index
                    });
                }
            }
        }

        foreach (var (sp, dp) in matched)
        {
            if (usedDestNames.Contains(dp.Name))
                continue;

            var hasPublicSetter = dp.SetMethod != null
                && dp.SetMethod.DeclaredAccessibility == Accessibility.Public;

            if (hasPublicSetter)
            {
                flow.Properties.Add(new PropertyFlow
                {
                    SourceProperty = sp.Name,
                    DestinationProperty = dp.Name,
                    Strategy = MappingStrategy.Direct
                });
                usedDestNames.Add(dp.Name);
            }
        }

        if (flow.Policy.EnableFlatten)
        {
            foreach (var kvp in destProps)
            {
                if (usedDestNames.Contains(kvp.Value.Name))
                    continue;

                var flattenPath = FlattenResolver.ResolvePath(sourceType, kvp.Value.Name, kvp.Value.Type);
                if (flattenPath != null)
                {
                    flow.Properties.Add(new PropertyFlow
                    {
                        SourceProperty = flattenPath.Segments.Last(),
                        DestinationProperty = kvp.Value.Name,
                        SourcePath = flattenPath.FullPath,
                        Strategy = MappingStrategy.Flatten
                    });
                    usedDestNames.Add(kvp.Value.Name);
                }
            }
        }

        return flow;
    }

    public static bool IsComplexType(ITypeSymbol type)
    {
        if (type.SpecialType != SpecialType.None)
            return false;
        if (type.TypeKind == TypeKind.Enum)
            return false;
        if (type is IArrayTypeSymbol)
            return false;
        return true;
    }
}
