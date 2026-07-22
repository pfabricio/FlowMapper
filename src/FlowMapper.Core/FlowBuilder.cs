using System.Collections.Concurrent;
using System.Reflection;

namespace FlowMapper.Core;

public class FlowBuilder
{
    private static readonly ConcurrentDictionary<(Type, Type), Flow> _cache = new();

    public Flow Build(Type sourceType, Type destinationType, ProfileDefinition? profile = null)
    {
        var key = (sourceType, destinationType);
        return _cache.GetOrAdd(key, _ => BuildCore(sourceType, destinationType, profile));
    }

    private Flow BuildCore(Type sourceType, Type destinationType, ProfileDefinition? profile)
    {
        var flow = new Flow
        {
            Name = $"{sourceType.Name}To{destinationType.Name}Mapper",
            Signature = new FlowSignature { SourceType = sourceType, DestinationType = destinationType },
            Policy = profile?.Policy
        };

        var sourceProps = sourceType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var destProps = destinationType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        var explicitMappings = profile?.Registrations
            .Where(r => r.SourceType == sourceType && r.DestinationType == destinationType && r.Expression != null)
            .Select(r => r.Expression)
            .OfType<object>()
            .SelectMany(e =>
            {
                var field = e.GetType().GetField("ExplicitMappings", BindingFlags.Instance | BindingFlags.NonPublic);
                return (IEnumerable<ExplicitMapping>)(field?.GetValue(e) ?? Enumerable.Empty<ExplicitMapping>());
            })
            .ToList() ?? new();

        var reverseFlag = profile?.Registrations
            .Any(r => r.SourceType == sourceType && r.DestinationType == destinationType && 
                (bool?)r.Expression?.GetType().GetField("ReverseMapped", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(r.Expression) == true) ?? false;

        flow.IsReverse = reverseFlag;

        foreach (var destProp in destProps)
        {
            if (!destProp.CanWrite) continue;

            var explicitMapping = explicitMappings.FirstOrDefault(m => m.DestinationProperty == destProp.Name);
            if (explicitMapping?.IsIgnored == true) continue;

            if (explicitMapping?.IsPathMapping == true)
            {
                var sourceProp = sourceProps.FirstOrDefault(p => p.Name == explicitMapping?.SourceProperty);
                if (sourceProp != null)
                {
                    flow.Properties.Add(new PropertyFlow
                    {
                        SourceProperty = sourceProp.Name,
                        DestinationProperty = destProp.Name,
                        SourceType = sourceProp.PropertyType,
                        DestinationType = destProp.PropertyType,
                        IsPathMapping = true,
                        PathSegments = explicitMapping?.PathSegments ?? new(),
                        MapFromExpression = explicitMapping?.MapFromExpression
                    });
                }
                continue;
            }

            if (explicitMapping != null && explicitMapping.SourceProperty != destProp.Name)
            {
                var sourceProp = sourceProps.FirstOrDefault(p => p.Name == explicitMapping.SourceProperty);
                if (sourceProp != null)
                {
                    flow.Properties.Add(new PropertyFlow
                    {
                        SourceProperty = sourceProp.Name,
                        DestinationProperty = destProp.Name,
                        SourceType = sourceProp.PropertyType,
                        DestinationType = destProp.PropertyType,
                        MapFromExpression = explicitMapping.MapFromExpression
                    });
                }
                continue;
            }

            var sourceProp2 = sourceProps.FirstOrDefault(p => p.Name == destProp.Name);
            if (sourceProp2 != null)
            {
                if (IsComplexType(sourceProp2.PropertyType) && IsComplexType(destProp.PropertyType) && destProp.PropertyType != typeof(string))
                {
                    var childFlow = BuildCore(sourceProp2.PropertyType, destProp.PropertyType, profile);
                    flow.NestedFlows.Add(new NestedFlow
                    {
                        ParentProperty = destProp.Name,
                        ChildFlow = childFlow
                    });
                }
                else
                {
                    flow.Properties.Add(new PropertyFlow
                    {
                        SourceProperty = sourceProp2.Name,
                        DestinationProperty = destProp.Name,
                        SourceType = sourceProp2.PropertyType,
                        DestinationType = destProp.PropertyType
                    });
                }
            }
        }

        return flow;
    }

    private static bool IsComplexType(Type type) =>
        type is { IsClass: true, IsPrimitive: false } &&
        type != typeof(string) &&
        type != typeof(decimal) &&
        type != typeof(DateTime) &&
        type != typeof(Guid) &&
        !type.IsEnum &&
        Nullable.GetUnderlyingType(type) == null;
}
