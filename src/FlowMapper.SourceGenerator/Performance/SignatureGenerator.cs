using FlowMapper.Core;
using FlowMapper.SourceGenerator.Models;

namespace FlowMapper.SourceGenerator.Performance;

public static class SignatureGenerator
{
    public static FlowSignature Generate(Flow flow)
    {
        var propHash = string.Join("|",
            flow.Properties
                .OrderBy(p => p.DestinationProperty)
                .Select(p => $"{p.DestinationProperty}:{p.SourceProperty}:{p.Strategy}"));

        return new FlowSignature
        {
            SourceTypeId = flow.SourceType,
            DestinationTypeId = flow.DestinationType,
            ProfileName = flow.ProfileName,
            PolicyHash = flow.Policy.ToString(),
            PropertyHash = propHash
        };
    }

    public static FlowSignature GenerateFromCandidate(MapperDefinition candidate)
    {
        return new FlowSignature
        {
            SourceTypeId = candidate.SourceType.ToDisplayString(),
            DestinationTypeId = candidate.DestinationType.ToDisplayString(),
            ProfileName = "Default",
            PolicyHash = "v1",
            PropertyHash = string.Empty
        };
    }
}
