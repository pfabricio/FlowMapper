namespace FlowMapper.Core;

/// <summary>Represents a resolved flatten path from a source type to a leaf property.
/// Produced by <c>FlattenResolver</c> during DFS traversal of complex source types.</summary>
public class FlattenPath
{
    /// <summary>Dot-separated full path (e.g., "Address.Street").</summary>
    public string FullPath { get; set; } = string.Empty;

    /// <summary>Individual path segments (e.g., ["Address", "Street"]).</summary>
    public List<string> Segments { get; set; } = new();

    /// <summary>The name of the leaf (target) property.</summary>
    public string TargetProperty { get; set; } = string.Empty;
}
