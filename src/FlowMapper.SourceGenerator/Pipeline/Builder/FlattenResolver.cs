using Microsoft.CodeAnalysis;

namespace FlowMapper.SourceGenerator.Pipeline.Builder;

internal sealed class LeafPath
{
    public string FullPath { get; set; } = string.Empty;
    public List<string> Segments { get; set; } = new();
    public string TargetProperty { get; set; } = string.Empty;
    public ITypeSymbol LeafType { get; set; } = null!;
}

public static class FlattenResolver
{
    public static (string FullPath, List<string> Segments)? ResolvePath(
        ITypeSymbol sourceType,
        string targetPropertyName,
        ITypeSymbol targetType)
    {
        var leaves = new List<LeafPath>();
        var visited = new HashSet<string>();
        DfsFindLeaves(sourceType, new List<string>(), leaves, visited);

        var matches = leaves
            .Where(p =>
            {
                var candidateName = string.Join("", p.Segments);
                return string.Equals(candidateName, targetPropertyName, StringComparison.Ordinal) &&
                       SymbolEqualityComparer.Default.Equals(p.LeafType, targetType);
            })
            .ToList();

        if (matches.Count == 1)
        {
            var match = matches[0];
            return (match.FullPath, match.Segments);
        }

        return null;
    }

    private static void DfsFindLeaves(
        ITypeSymbol type,
        List<string> segments,
        List<LeafPath> leaves,
        HashSet<string> visited)
    {
        var typeKey = type.ToString();
        if (!visited.Add(typeKey))
            return;

        foreach (var member in type.GetMembers().OfType<IPropertySymbol>())
        {
            segments.Add(member.Name);

            if (IsSimpleType(member.Type))
            {
                leaves.Add(new LeafPath
                {
                    FullPath = string.Join(".", segments),
                    Segments = new List<string>(segments),
                    TargetProperty = member.Name,
                    LeafType = member.Type
                });
            }
            else if (member.Type is INamedTypeSymbol namedType)
            {
                DfsFindLeaves(namedType, segments, leaves, visited);
            }

            segments.RemoveAt(segments.Count - 1);
        }
    }

    private static bool IsSimpleType(ITypeSymbol type)
    {
        return type.SpecialType != SpecialType.None
            || type.TypeKind == TypeKind.Enum
            || type is IArrayTypeSymbol;
    }
}