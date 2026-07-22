using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace FlowMapper.SourceGenerator;

[Generator]
public class FlowMapperGenerator : IIncrementalGenerator
{
    private const string ProfileDefinitionFullName = "FlowMapper.Core.ProfileDefinition";
    private const string MapperInterfaceFullName = "FlowMapper.Abstractions.IMapper";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var profiles = context.SyntaxProvider.CreateSyntaxProvider(
            predicate: IsCandidate,
            transform: TransformCandidate
        ).Where(m => m != null)!;

        context.RegisterSourceOutput(profiles, GenerateCode);
    }

    private static bool IsCandidate(SyntaxNode node, CancellationToken ct)
    {
        return node is ClassDeclarationSyntax cds &&
               cds.BaseList?.Types.Any(t => t.Type is IdentifierNameSyntax ins &&
                   (ins.Identifier.Text == "ProfileDefinition" ||
                    ins.Identifier.Text == "ProfileDefinition<T>" ||
                    cds.AttributeLists.Any(a => a.Attributes.Any(
                        at => at.Name.ToString().Contains("Profile"))))) == true;
    }

    private static ProfileInfo? TransformCandidate(GeneratorSyntaxContext context, CancellationToken ct)
    {
        var classDecl = (ClassDeclarationSyntax)context.Node;
        var semanticModel = context.SemanticModel;
        var classSymbol = semanticModel.GetDeclaredSymbol(classDecl, ct) as INamedTypeSymbol;
        if (classSymbol == null) return null;

        var profileName = classSymbol.Name;
        var ns = classSymbol.ContainingNamespace?.ToDisplayString() ?? "FlowMapper.Profiles";
        var registrations = new List<MappingInfo>();

        var members = classDecl.Members.OfType<ConstructorDeclarationSyntax>().ToList();
        foreach (var ctor in members)
        {
            var invocations = ctor.DescendantNodes().OfType<InvocationExpressionSyntax>();
            foreach (var invocation in invocations)
            {
                var methodSymbol = semanticModel.GetSymbolInfo(invocation, ct).Symbol as IMethodSymbol;
                if (methodSymbol == null) continue;

                if (methodSymbol.Name == "CreateMap" && methodSymbol.TypeParameters.Length == 2)
                {
                    var sourceType = methodSymbol.TypeArguments[0];
                    var destType = methodSymbol.TypeArguments[1];
                    var mapping = ExtractMappingConfig(invocation, sourceType, destType, semanticModel, ct);
                    registrations.Add(mapping);
                }
            }
        }

        if (registrations.Count == 0) return null;

        return new ProfileInfo
        {
            Namespace = ns,
            ProfileName = profileName,
            Mappings = registrations
        };
    }

    private static MappingInfo ExtractMappingConfig(
        InvocationExpressionSyntax invocation,
        ITypeSymbol sourceType,
        ITypeSymbol destType,
        SemanticModel semanticModel,
        CancellationToken ct)
    {
        var mapping = new MappingInfo
        {
            SourceTypeName = sourceType.ToDisplayString(),
            SourceTypeGlobalName = FormatGlobalName(sourceType),
            DestTypeName = destType.ToDisplayString(),
            DestTypeGlobalName = FormatGlobalName(destType),
            SimpleSourceName = sourceType.Name,
            SimpleDestName = destType.Name,
            MapperName = $"{sourceType.Name}To{destType.Name}Mapper",
            PropertyMappings = new List<PropertyMappingInfo>(),
            ReverseMapped = false,
            ForPathMappings = new List<ForPathMappingInfo>()
        };

        var chain = GetFluentChain(invocation);
        foreach (var call in chain)
        {
            var symbol = semanticModel.GetSymbolInfo(call, ct).Symbol as IMethodSymbol;
            if (symbol == null) continue;

            if (symbol.Name == "ForMember")
                ExtractForMember(call, mapping, semanticModel, ct);
            else if (symbol.Name == "ForPath")
                ExtractForPath(call, mapping, semanticModel, ct);
            else if (symbol.Name == "ReverseMap")
                mapping.ReverseMapped = true;
        }

        var sourceMembers = sourceType.GetMembers().OfType<IPropertySymbol>()
            .Where(p => p.DeclaredAccessibility == Accessibility.Public && !p.IsStatic);
        var destMembers = destType.GetMembers().OfType<IPropertySymbol>()
            .Where(p => p.DeclaredAccessibility == Accessibility.Public && !p.IsStatic && p.SetMethod != null);

        foreach (var destProp in destMembers)
        {
            var existing = mapping.PropertyMappings.FirstOrDefault(m => m.DestinationProperty == destProp.Name);
            if (existing != null) continue;

            var sourceProp = sourceMembers.FirstOrDefault(p => p.Name == destProp.Name);
            if (sourceProp != null)
            {
                mapping.PropertyMappings.Add(new PropertyMappingInfo
                {
                    SourceProperty = sourceProp.Name,
                    DestinationProperty = destProp.Name,
                    SourceType = sourceProp.Type.ToDisplayString(),
                    DestinationType = destProp.Type.ToDisplayString()
                });
            }
        }

        return mapping;
    }

    private static void ExtractForMember(
        InvocationExpressionSyntax invocation,
        MappingInfo mapping,
        SemanticModel semanticModel,
        CancellationToken ct)
    {
        if (invocation.ArgumentList.Arguments.Count < 2) return;

        var destExpr = invocation.ArgumentList.Arguments[0].Expression;
        var destMember = ExtractMemberName(destExpr);
        if (destMember == null) return;

        var optArg = invocation.ArgumentList.Arguments[1].Expression;
        var isIgnored = false;
        var sourceExpr = destMember;

        var lambdaArgs = optArg.DescendantNodes().OfType<LambdaExpressionSyntax>().ToList();
        foreach (var lambda in lambdaArgs)
        {
            if (lambda.Body is ExpressionSyntax bodyExpr)
                sourceExpr = ExtractMemberName(bodyExpr);
        }

        if (optArg.DescendantNodes().Any(n => n is IdentifierNameSyntax ins && ins.Identifier.Text == "Ignore"))
            isIgnored = true;

        mapping.PropertyMappings.Add(new PropertyMappingInfo
        {
            SourceProperty = sourceExpr ?? destMember,
            DestinationProperty = destMember,
            IsIgnored = isIgnored,
            MapFromExpression = sourceExpr
        });
    }

    private static string? ExtractMemberName(ExpressionSyntax? expr)
    {
        if (expr == null) return null;

        var body = expr is LambdaExpressionSyntax lambda ? lambda.Body : expr;

        if (body is MemberAccessExpressionSyntax ma)
            return ma.Name.Identifier.Text;
        if (body is IdentifierNameSyntax ins)
            return ins.Identifier.Text;
        return null;
    }

    private static List<string> ExtractPathSegments(ExpressionSyntax? expr)
    {
        var segments = new List<string>();
        if (expr == null) return segments;

        SyntaxNode? current = expr;
        while (current is MemberAccessExpressionSyntax ma)
        {
            segments.Add(ma.Name.Identifier.Text);
            current = ma.Expression;
        }
        segments.Reverse();
        return segments;
    }

    private static void GenerateCode(SourceProductionContext context, ProfileInfo? profile)
    {
        if (profile == null) return;

        var source = GenerateMapperSource(profile);
        context.AddSource($"{profile.ProfileName}_Mappers.g.cs", SourceText.From(source, Encoding.UTF8));
    }

    private static string GenerateMapperSource(ProfileInfo profile)
    {
        var sb = new StringBuilder();
        sb.AppendLine("// <auto-generated>");
        sb.AppendLine("#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604");
        sb.AppendLine();
        sb.AppendLine($"namespace {profile.Namespace}.Generated;");
        sb.AppendLine();

        foreach (var mapping in profile.Mappings)
        {
            GenerateMapperClass(sb, mapping);
        }

        return sb.ToString();
    }

    private static void GenerateMapperClass(StringBuilder sb, MappingInfo mapping)
    {
        var srcType = mapping.SourceTypeGlobalName;
        var dstType = mapping.DestTypeGlobalName;

        sb.AppendLine($"public partial class {mapping.MapperName} : FlowMapper.Abstractions.IMapper<{srcType}, {dstType}>");
        sb.AppendLine("{");
        sb.AppendLine($"    public {dstType} Map({srcType} source)");
        sb.AppendLine("    {");
        sb.AppendLine($"        var target = new {dstType}();");

        foreach (var prop in mapping.PropertyMappings)
        {
            if (prop.IsIgnored) continue;

            var src = prop.SourceProperty;
            var dst = prop.DestinationProperty;
            sb.AppendLine($"        target.{dst} = source.{src};");
        }

        foreach (var forPath in mapping.ForPathMappings)
        {
            var segments = forPath.PathSegments;
            if (segments.Count <= 1)
            {
                sb.AppendLine($"        target.{segments[0]} = source.{forPath.SourceProperty};");
            }
            else
            {
                for (int i = 0; i < segments.Count - 1; i++)
                {
                    var prefix = string.Join(".", segments.Take(i + 1));
                    sb.AppendLine($"        target.{prefix} ??= new global::FlowMapper.Generated.AutoCreated_{segments[i]}();");
                }
                sb.AppendLine($"        target.{forPath.DestinationPath} = source.{forPath.SourceProperty};");
            }
        }

        sb.AppendLine("        return target;");
        sb.AppendLine("    }");
        sb.AppendLine("}");
        sb.AppendLine();

        if (mapping.ReverseMapped)
        {
            var reverseMapperName = $"{mapping.SimpleDestName}To{mapping.SimpleSourceName}Mapper";
            sb.AppendLine($"public partial class {reverseMapperName} : FlowMapper.Abstractions.IMapper<{dstType}, {srcType}>");
            sb.AppendLine("{");
            sb.AppendLine($"    public {srcType} Map({dstType} source)");
            sb.AppendLine("    {");
            sb.AppendLine($"        var target = new {srcType}();");

            foreach (var prop in mapping.PropertyMappings.Where(p => !p.IsIgnored))
            {
                sb.AppendLine($"        target.{prop.DestinationProperty} = source.{prop.SourceProperty};");
            }

            sb.AppendLine("        return target;");
            sb.AppendLine("    }");
            sb.AppendLine("}");
            sb.AppendLine();
        }
    }

    private static void ExtractForPath(
        InvocationExpressionSyntax invocation,
        MappingInfo mapping,
        SemanticModel semanticModel,
        CancellationToken ct)
    {
        if (invocation.ArgumentList.Arguments.Count < 2) return;

        var destExpr = invocation.ArgumentList.Arguments[0].Expression;
        var pathSegments = ExtractPathSegments(destExpr);
        if (pathSegments.Count == 0) return;

        var optArg = invocation.ArgumentList.Arguments[1].Expression;
        var sourceExpr = pathSegments.Last();

        var lambdaArgs = optArg.DescendantNodes().OfType<LambdaExpressionSyntax>().ToList();
        foreach (var lambda in lambdaArgs)
        {
            if (lambda.Body is ExpressionSyntax bodyExpr)
            {
                var member = ExtractMemberName(bodyExpr);
                if (member != null) sourceExpr = member;
            }
        }

        mapping.ForPathMappings.Add(new ForPathMappingInfo
        {
            PathSegments = pathSegments,
            SourceProperty = sourceExpr,
            DestinationPath = string.Join(".", pathSegments)
        });
    }

    private static List<InvocationExpressionSyntax> GetFluentChain(InvocationExpressionSyntax createMapCall)
    {
        var chain = new List<InvocationExpressionSyntax>();

        SyntaxNode? current = createMapCall.Parent;
        while (current != null)
        {
            if (current is InvocationExpressionSyntax invocation)
            {
                chain.Add(invocation);
                current = invocation.Parent;
            }
            else if (current is MemberAccessExpressionSyntax ma)
            {
                current = ma.Parent;
            }
            else if (current is ArgumentListSyntax || current is ExpressionStatementSyntax)
            {
                current = current.Parent;
            }
            else
            {
                break;
            }
        }

        chain.Reverse();
        return chain;
    }

    private static string FormatGlobalName(ITypeSymbol type)
    {
        return type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
    }
}

internal class ProfileInfo
{
    public string Namespace { get; set; } = string.Empty;
    public string ProfileName { get; set; } = string.Empty;
    public List<MappingInfo> Mappings { get; set; } = new();
}

internal class MappingInfo
{
    public string SourceTypeName { get; set; } = string.Empty;
    public string SourceTypeGlobalName { get; set; } = string.Empty;
    public string DestTypeName { get; set; } = string.Empty;
    public string DestTypeGlobalName { get; set; } = string.Empty;
    public string SimpleSourceName { get; set; } = string.Empty;
    public string SimpleDestName { get; set; } = string.Empty;
    public string MapperName { get; set; } = string.Empty;
    public List<PropertyMappingInfo> PropertyMappings { get; set; } = new();
    public List<ForPathMappingInfo> ForPathMappings { get; set; } = new();
    public bool ReverseMapped { get; set; }
}

internal class PropertyMappingInfo
{
    public string SourceProperty { get; set; } = string.Empty;
    public string DestinationProperty { get; set; } = string.Empty;
    public string SourceType { get; set; } = string.Empty;
    public string DestinationType { get; set; } = string.Empty;
    public bool IsIgnored { get; set; }
    public string? MapFromExpression { get; set; }
}

internal class ForPathMappingInfo
{
    public List<string> PathSegments { get; set; } = new();
    public string SourceProperty { get; set; } = string.Empty;
    public string DestinationPath { get; set; } = string.Empty;
}
