namespace FlowMapper.Core;

/// <summary>Uniquely identifies a mapping combination for caching purposes.
/// Combines source/destination types, profile name, policy, and property resolution fingerprints.</summary>
public class FlowSignature
{
    /// <summary>Source type identity string.</summary>
    public string SourceTypeId { get; set; } = string.Empty;

    /// <summary>Destination type identity string.</summary>
    public string DestinationTypeId { get; set; } = string.Empty;

    /// <summary>Profile name the mapping belongs to.</summary>
    public string ProfileName { get; set; } = string.Empty;

    /// <summary>Hash of policy settings affecting mapping behavior.</summary>
    public string PolicyHash { get; set; } = string.Empty;

    /// <summary>Hash of all property names and their resolution strategies, ordered for deterministic comparison.</summary>
    public string PropertyHash { get; set; } = string.Empty;

    /// <summary>Builds a cache key string combining all signature components.</summary>
    public string ToCacheKey()
    {
        return $"{ProfileName}|{SourceTypeId}|{DestinationTypeId}|{PolicyHash}|{PropertyHash}";
    }
}
