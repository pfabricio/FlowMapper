using Microsoft.CodeAnalysis;
using FlowMapper.Core;

namespace FlowMapper.SourceGenerator.Pipeline.Builder;

public static class ConstructorResolver
{
    public static List<ConstructorBinding>? Resolve(
        INamedTypeSymbol sourceType,
        INamedTypeSymbol destType,
        IReadOnlyCollection<string> alreadyMappedDestNames)
    {
        var constructors = destType
            .GetMembers()
            .OfType<IMethodSymbol>()
            .Where(c => c.MethodKind == MethodKind.Constructor && c.DeclaredAccessibility == Accessibility.Public)
            .ToList();

        var sourceProps = sourceType
            .GetMembers()
            .OfType<IPropertySymbol>()
            .ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);

        IMethodSymbol? bestCtor = null;
        var bestScore = -1;

        foreach (var ctor in constructors)
        {
            if (ctor.Parameters.Length == 0)
                continue;

            var score = ctor.Parameters.Count(p =>
                sourceProps.TryGetValue(p.Name, out var sp) &&
                SymbolEqualityComparer.Default.Equals(sp.Type, p.Type));

            if (score > bestScore)
            {
                bestScore = score;
                bestCtor = ctor;
            }
        }

        if (bestCtor == null || bestScore == 0)
            return null;

        var bindings = new List<ConstructorBinding>();
        var mappedNames = new HashSet<string>(alreadyMappedDestNames, StringComparer.OrdinalIgnoreCase);

        for (int i = 0; i < bestCtor.Parameters.Length; i++)
        {
            var param = bestCtor.Parameters[i];
            if (sourceProps.TryGetValue(param.Name, out var sp) &&
                SymbolEqualityComparer.Default.Equals(sp.Type, param.Type))
            {
                bindings.Add(new ConstructorBinding
                {
                    ParameterName = param.Name,
                    SourceProperty = sp.Name,
                    Index = i
                });
            }
        }

        return bindings;
    }
}
