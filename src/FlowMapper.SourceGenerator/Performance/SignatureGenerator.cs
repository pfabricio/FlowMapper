using FlowMapper.SourceGenerator.Models;

namespace FlowMapper.SourceGenerator.Performance;

public static class SignatureGenerator
{
    public static string GeneratePropertyHash(FlowDescriptor flow)
    {
        return string.Join("|",
            flow.Properties
                .OrderBy(p => p.DestinationProperty)
                .Select(p => $"{p.DestinationProperty}:{p.SourceProperty}:{p.Strategy}"));
    }

    public static string GenerateCacheKey(MapperDefinition candidate)
    {
        return $"{candidate.SourceType.ToDisplayString()}|{candidate.DestinationType.ToDisplayString()}|{candidate.ProfileName}";
    }
}