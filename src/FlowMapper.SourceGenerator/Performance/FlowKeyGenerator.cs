using Microsoft.CodeAnalysis;

namespace FlowMapper.SourceGenerator.Performance;

public static class FlowKeyGenerator
{
    public static string Create(INamedTypeSymbol source, INamedTypeSymbol dest)
    {
        return $"{source.ToDisplayString()}|{dest.ToDisplayString()}";
    }
}
