using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using FlowMapper.SourceGenerator.Models;
using FlowMapper.SourceGenerator.Pipeline;
using FlowMapper.SourceGenerator.Pipeline.Generator;

namespace FlowMapper.SourceGenerator;

[Generator]
public class FlowMapperGenerator : IIncrementalGenerator
{
    private const string ProfileDefinitionFullName = "FlowMapper.Core.ProfileDefinition";
    private const string FtsProfileDefinitionFullName = "FlowMapper.FullTextSearch.FtsProfileDefinition";
    private const string MapAttributeName = "MapAttribute";

    private static readonly DiagnosticDescriptor FtsPropertyNotConfigured = new(
        "FM5001",
        "Property not configured for Full-Text Search",
        "Property '{0}' on '{1}' is not configured with HasFullTextIndex and may not be searchable",
        "FullTextSearch",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor FtsIncompatibleColumn = new(
        "FM5002",
        "Column type incompatible with Full-Text Search",
        "Property '{0}' on '{1}' is of type '{2}' which is not compatible with Full-Text Search; only string is supported",
        "FullTextSearch",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var mapperDefs = context.SyntaxProvider.CreateSyntaxProvider(
            predicate: IsCandidate,
            transform: TransformCandidate
        ).Where(m => m != null)!;

        var ftsInfos = context.SyntaxProvider.CreateSyntaxProvider(
            predicate: IsFtsCandidate,
            transform: TransformFtsCandidate
        ).Where(f => f != null)!;

        var combined = mapperDefs.Collect().Combine(ftsInfos.Collect());

        context.RegisterSourceOutput(combined, GenerateCode);
    }

    private static bool IsCandidate(SyntaxNode node, CancellationToken ct)
    {
        if (node is ClassDeclarationSyntax cds)
        {
            if (cds.BaseList?.Types.Any(t => t.Type.ToString().Contains("ProfileDefinition")) == true)
                return true;

            if (cds.AttributeLists.Any(a => a.Attributes.Any(at =>
                at.Name.ToString().Contains("Map") || at.Name.ToString().Contains("Profile"))))
                return true;

            if (cds.BaseList?.Types.Any(t => t.Type.ToString().Contains("IMapper")) == true)
                return true;
        }
        return false;
    }

    private static bool IsFtsCandidate(SyntaxNode node, CancellationToken ct)
    {
        if (node is ClassDeclarationSyntax cds)
        {
            return cds.BaseList?.Types.Any(t =>
                t.Type.ToString().Contains("FtsProfileDefinition")) == true;
        }
        return false;
    }

    private static List<MapperDefinition>? TransformCandidate(GeneratorSyntaxContext context, CancellationToken ct)
    {
        var classDecl = (ClassDeclarationSyntax)context.Node;
        var semanticModel = context.SemanticModel;
        var classSymbol = semanticModel.GetDeclaredSymbol(classDecl, ct) as INamedTypeSymbol;
        if (classSymbol == null) return null;

        var definitions = new List<MapperDefinition>();

        var baseType = classSymbol.BaseType;
        if (baseType != null && (baseType.ToDisplayString() == ProfileDefinitionFullName || baseType.Name == "ProfileDefinition"))
        {
            var compilation = semanticModel.Compilation;
            var profileDefs = MapperDefinitionFactory.CreateFromProfile(classSymbol, compilation);
            definitions.AddRange(profileDefs);
        }

        foreach (var attr in classSymbol.GetAttributes())
        {
            if (attr.AttributeClass?.Name == MapAttributeName)
            {
                var def = MapperDefinitionFactory.Create(classSymbol, attr);
                definitions.Add(def);
            }
        }

        return definitions.Count > 0 ? definitions : null;
    }

    private static FtsProfileInfo? TransformFtsCandidate(GeneratorSyntaxContext context, CancellationToken ct)
    {
        var classDecl = (ClassDeclarationSyntax)context.Node;
        var semanticModel = context.SemanticModel;
        var classSymbol = semanticModel.GetDeclaredSymbol(classDecl, ct) as INamedTypeSymbol;
        if (classSymbol == null) return null;

        var baseType = classSymbol.BaseType;
        if (baseType == null) return null;
        if (baseType.ToDisplayString() != FtsProfileDefinitionFullName && baseType.Name != "FtsProfileDefinition")
            return null;

        var info = new FtsProfileInfo();

        var ctor = classDecl.Members
            .OfType<ConstructorDeclarationSyntax>()
            .FirstOrDefault();
        if (ctor == null) return info;

        var invocations = ctor.DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .ToList();

        foreach (var invocation in invocations)
        {
            var methodName = invocation.Expression switch
            {
                MemberAccessExpressionSyntax ma => ma.Name.Identifier.Text,
                SimpleNameSyntax sn => sn.Identifier.Text,
                _ => null
            };

            if (methodName == "Entity")
            {
                var symbolInfo = semanticModel.GetSymbolInfo(invocation);
                if (symbolInfo.Symbol is IMethodSymbol methodSymbol
                    && methodSymbol.TypeArguments.Length == 1
                    && methodSymbol.TypeArguments[0] is INamedTypeSymbol entityType)
                {
                    info.EntityTypes.Add(entityType);
                }
            }
        }

        foreach (var invocation in invocations)
        {
            var methodName = invocation.Expression switch
            {
                MemberAccessExpressionSyntax ma => ma.Name.Identifier.Text,
                SimpleNameSyntax sn => sn.Identifier.Text,
                _ => null
            };

            if (methodName == "HasFullTextIndex")
            {
                if (invocation.ArgumentList.Arguments.Count >= 1)
                {
                    var arg = invocation.ArgumentList.Arguments[0].Expression;
                    var propName = ExtractPropertyNameFromExpression(arg);
                    if (propName != null)
                    {
                        var entityType = info.EntityTypes.Count > 0
                            ? info.EntityTypes[info.EntityTypes.Count - 1]
                            : null;
                        var resolved = ResolvePropertyTypeInfo(entityType, propName, semanticModel, arg);
                        info.ConfiguredProperties.Add(resolved);
                    }
                }
            }
        }

        return info;
    }

    private static FtsConfiguredProperty ResolvePropertyTypeInfo(
        INamedTypeSymbol? entityType, string propName, SemanticModel semanticModel, ExpressionSyntax arg)
    {
        if (entityType != null)
        {
            var member = entityType.GetMembers(propName).FirstOrDefault();
            if (member is IPropertySymbol prop)
            {
                var typeName = prop.Type.ToDisplayString();
                return new FtsConfiguredProperty
                {
                    PropertyName = propName,
                    TypeName = typeName,
                    IsString = prop.Type.SpecialType == SpecialType.System_String,
                    TypeUnresolved = false
                };
            }
        }

        var typeInfo = semanticModel.GetTypeInfo(arg);
        var fallbackType = typeInfo.Type;
        var fallbackName = fallbackType?.ToDisplayString() ?? "";
        return new FtsConfiguredProperty
        {
            PropertyName = propName,
            TypeName = fallbackName,
            IsString = fallbackType?.SpecialType == SpecialType.System_String,
            TypeUnresolved = fallbackType is null || fallbackName is "" or "?" or "unknown"
        };
    }

    private static void GenerateCode(
        SourceProductionContext context,
        (ImmutableArray<List<MapperDefinition>?> MapperDefs, ImmutableArray<FtsProfileInfo?> FtsInfos) input)
    {
        var (allDefs, ftsInfos) = input;

        var definitions = new List<MapperDefinition>();
        foreach (var defList in allDefs)
        {
            if (defList != null)
                definitions.AddRange(defList);
        }

        var ftsProfiles = new List<FtsProfileInfo>();
        foreach (var info in ftsInfos)
        {
            if (info != null)
                ftsProfiles.Add(info);
        }

        foreach (var ftsProfile in ftsProfiles)
        {
            AnalyzeFtsProfile(context, ftsProfile);
        }

        if (definitions.Count == 0) return;

        var model = FlowPipeline.Execute(definitions);

        var source = FlowCodeGenerator.Generate(model);
        context.AddSource("FlowMapper_Mappers.g.cs", SourceText.From(source, Encoding.UTF8));
    }

    private static void AnalyzeFtsProfile(SourceProductionContext context, FtsProfileInfo profile)
    {
        var configuredNames = new HashSet<string>(
            profile.ConfiguredProperties.Select(p => p.PropertyName));

        foreach (var entityType in profile.EntityTypes)
        {
            var stringMembers = entityType.GetMembers()
                .OfType<IPropertySymbol>()
                .Where(p => p.Type.SpecialType == SpecialType.System_String &&
                            p.DeclaredAccessibility == Accessibility.Public);

            foreach (var member in stringMembers)
            {
                if (!configuredNames.Contains(member.Name))
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(FtsPropertyNotConfigured,
                        member.Locations.FirstOrDefault(),
                        member.Name, entityType.Name));
                }
            }
        }

        foreach (var prop in profile.ConfiguredProperties)
        {
            if (!prop.TypeUnresolved && !prop.IsString)
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(FtsIncompatibleColumn,
                    Location.None,
                    prop.PropertyName,
                    profile.EntityTypes.FirstOrDefault()?.Name ?? "?",
                    prop.TypeName));
            }
        }
    }

    private static string? ExtractPropertyNameFromExpression(ExpressionSyntax expr)
    {
        if (expr is SimpleLambdaExpressionSyntax lambda)
        {
            if (lambda.Body is MemberAccessExpressionSyntax memberAccess)
                return memberAccess.Name.Identifier.Text;
        }

        if (expr is ParenthesizedLambdaExpressionSyntax parenLambda)
        {
            if (parenLambda.Body is MemberAccessExpressionSyntax memberAccess)
                return memberAccess.Name.Identifier.Text;
        }

        return null;
    }
}

public class FtsProfileInfo
{
    public List<FtsConfiguredProperty> ConfiguredProperties { get; } = new();
    public List<INamedTypeSymbol> EntityTypes { get; } = new();
}

public class FtsConfiguredProperty
{
    public string PropertyName { get; set; } = string.Empty;
    public string TypeName { get; set; } = string.Empty;
    public bool IsString { get; set; }
    public bool TypeUnresolved { get; set; }
}
