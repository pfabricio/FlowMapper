namespace FlowMapper.Abstractions;

/// <summary>Controls how strictly the mapper validates that all destination properties are mapped.</summary>
public enum StrictnessLevel
{
    /// <summary>Unmapped properties are silently allowed.</summary>
    None,

    /// <summary>Unmapped properties produce compile-time warnings (FM0001).</summary>
    Warning,

    /// <summary>Unmapped properties produce compile-time errors, failing the build.</summary>
    Error
}
