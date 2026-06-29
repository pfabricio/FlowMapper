using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using FlowMapper.Abstractions;
using FlowMapper.Core;
using FlowMapper.SourceGenerator.Models;

namespace FlowMapper.SourceGenerator;

public static class MapperDefinitionFactory
{
    public static MapperDefinition Create(INamedTypeSymbol mapperType, AttributeData attribute)
    {
        if (attribute.AttributeClass is not { TypeArguments.Length: 2 } typeArgs)
            throw new InvalidOperationException("MapAttribute must have exactly 2 generic type arguments.");

        var sourceType = (INamedTypeSymbol)typeArgs.TypeArguments[0];
        var destType = (INamedTypeSymbol)typeArgs.TypeArguments[1];

        var profileName = "Default";
        MappingPolicy? profilePolicy = null;
        var ignoredProperties = new HashSet<string>();
        var explicitMappings = new List<ExplicitMappingInfo>();

        var profileAttr = mapperType
            .GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.Name == "FlowProfileAttribute");

        if (profileAttr != null)
        {
            if (profileAttr.ConstructorArguments.Length > 0)
                profileName = profileAttr.ConstructorArguments[0].Value?.ToString() ?? "Default";

            profilePolicy = new MappingPolicy();

            foreach (var namedArg in profileAttr.NamedArguments)
            {
                switch (namedArg.Key)
                {
                    case "EnableFlatten":
                        profilePolicy.EnableFlatten = (bool)namedArg.Value.Value!;
                        break;
                    case "PreferConstructor":
                        profilePolicy.PreferConstructor = (bool)namedArg.Value.Value!;
                        break;
                    case "Strictness":
                        profilePolicy.Strictness = (StrictnessLevel)(int)namedArg.Value.Value!;
                        break;
                }
            }
        }

        foreach (var attr in mapperType.GetAttributes())
        {
            switch (attr.AttributeClass?.Name)
            {
                case "MapPropertyAttribute":
                    if (attr.ConstructorArguments.Length >= 2)
                    {
                        var src = attr.ConstructorArguments[0].Value?.ToString();
                        var dst = attr.ConstructorArguments[1].Value?.ToString();
                        if (src != null && dst != null)
                        {
                            explicitMappings.Add(new ExplicitMappingInfo
                            {
                                SourceProperty = src,
                                DestinationProperty = dst
                            });
                        }
                    }
                    break;

                case "IgnoreMapAttribute":
                    if (attr.ConstructorArguments.Length >= 1)
                    {
                        var propName = attr.ConstructorArguments[0].Value?.ToString();
                        if (propName != null)
                            ignoredProperties.Add(propName);
                    }
                    break;
            }
        }

        return new MapperDefinition
        {
            SourceType = sourceType,
            DestinationType = destType,
            MapperType = mapperType,
            Attribute = attribute,
            ProfileName = profileName,
            ProfilePolicy = profilePolicy,
            IgnoredProperties = ignoredProperties,
            ExplicitMappings = explicitMappings
        };
    }

    public static List<MapperDefinition> CreateFromProfile(
        INamedTypeSymbol profileType,
        Compilation compilation)
    {
        var candidates = new List<MapperDefinition>();

        foreach (var syntaxRef in profileType.DeclaringSyntaxReferences)
        {
            var syntax = syntaxRef.GetSyntax();
            if (syntax is not ClassDeclarationSyntax classDecl) continue;

            var ctor = classDecl.Members
                .OfType<ConstructorDeclarationSyntax>()
                .FirstOrDefault();
            if (ctor == null) continue;

            var semanticModel = compilation.GetSemanticModel(ctor.SyntaxTree);

            var invocations = ctor.DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .Where(inv => inv.Expression is MemberAccessExpressionSyntax memberAccess
                           && memberAccess.Name.Identifier.Text == "CreateMap"
                           || inv.Expression is SimpleNameSyntax simpleName
                           && simpleName.Identifier.Text == "CreateMap");

            foreach (var invocation in invocations)
            {
                var symbolInfo = semanticModel.GetSymbolInfo(invocation);
                if (symbolInfo.Symbol is not IMethodSymbol methodSymbol) continue;

                if (methodSymbol.TypeArguments.Length != 2) continue;
                if (methodSymbol.TypeArguments[0] is not INamedTypeSymbol srcType) continue;
                if (methodSymbol.TypeArguments[1] is not INamedTypeSymbol dstType) continue;

                var policy = new MappingPolicy();
                var ignoredProperties = new HashSet<string>();
                var explicitMappings = new List<ExplicitMappingInfo>();
                string? afterMapMethod = null;
                string? constructUsingMethod = null;
                var mapperName = $"{srcType.Name}To{dstType.Name}Mapper";

                // Walk up to outermost invocation (root of fluent chain)
                var rootInvocation = invocation;
                while (rootInvocation.Parent is MemberAccessExpressionSyntax memberAccess
                       && memberAccess.Parent is InvocationExpressionSyntax parentInvocation)
                {
                    rootInvocation = parentInvocation;
                }

                var chain = CollectFluentCalls(rootInvocation);
                foreach (var call in chain)
                {
                    switch (call.MethodName)
                    {
                        case "Ignore":
                            if (call.Args.Count >= 1)
                            {
                                var propName = ExtractPropertyNameFromLambda(call.Args[0]);
                                if (propName != null)
                                    ignoredProperties.Add(propName);
                            }
                            break;

                        case "ForMember":
                            if (call.Args.Count >= 2)
                            {
                                var destProp = ExtractPropertyNameFromLambda(call.Args[0]);
                                if (destProp == null) continue;

                                var memberConfig = call.Args[1];
                                var mapFromExpr = ExtractMapFromExpression(memberConfig);
                                if (mapFromExpr != null)
                                {
                                    explicitMappings.Add(new ExplicitMappingInfo
                                    {
                                        DestinationProperty = destProp,
                                        SourceProperty = destProp,
                                        MapFromExpression = mapFromExpr
                                    });
                                }
                                else if (IsIgnoreCall(memberConfig))
                                {
                                    ignoredProperties.Add(destProp);
                                }
                                else
                                {
                                    var srcProp = ExtractPropertyNameFromLambda(memberConfig);
                                    if (srcProp != null)
                                    {
                                        explicitMappings.Add(new ExplicitMappingInfo
                                        {
                                            DestinationProperty = destProp,
                                            SourceProperty = srcProp
                                        });
                                    }
                                }
                            }
                            break;

                        case "UseConstructor":
                            policy.PreferConstructor = true;
                            break;

                        case "DisableFlatten":
                            policy.EnableFlatten = false;
                            break;

                        case "AfterMap":
                            if (call.Args.Count >= 1)
                                afterMapMethod = ExtractCallbackMethod(call.Args[0], "AfterMap");
                            break;

                        case "ConstructUsing":
                            if (call.Args.Count >= 1)
                                constructUsingMethod = ExtractCallbackMethod(call.Args[0], "ConstructUsing");
                            break;
                    }
                }

                candidates.Add(new MapperDefinition
                {
                    SourceType = srcType,
                    DestinationType = dstType,
                    MapperType = profileType,
                    Attribute = null!,
                    ProfileName = profileType.Name,
                    ProfilePolicy = policy,
                    IgnoredProperties = ignoredProperties,
                    ExplicitMappings = explicitMappings,
                    AfterMapMethod = afterMapMethod,
                    ConstructUsingMethod = constructUsingMethod,
                    MapperName = mapperName
                });
            }
        }

        return candidates;
    }

    private static List<(string MethodName, List<ExpressionSyntax> Args)> CollectFluentCalls(
        InvocationExpressionSyntax createMapInvocation)
    {
        var calls = new List<(string, List<ExpressionSyntax>)>();

        ExpressionSyntax current = createMapInvocation;

        while (current is InvocationExpressionSyntax inv)
        {
            if (inv.Expression is MemberAccessExpressionSyntax memberAccess)
            {
                calls.Insert(0, (memberAccess.Name.Identifier.Text, inv.ArgumentList.Arguments.Select(a => a.Expression).ToList()));
                current = memberAccess.Expression;
            }
            else if (inv.Expression is SimpleNameSyntax simpleName)
            {
                calls.Insert(0, (simpleName.Identifier.Text, inv.ArgumentList.Arguments.Select(a => a.Expression).ToList()));
                break;
            }
            else
            {
                break;
            }
        }

        return calls;
    }

    private static string? ExtractPropertyNameFromLambda(ExpressionSyntax expr)
    {
        if (expr is SimpleLambdaExpressionSyntax lambda)
        {
            if (lambda.Body is MemberAccessExpressionSyntax memberAccess)
                return memberAccess.Name.Identifier.Text;

            if (lambda.Body is InvocationExpressionSyntax inv
                && inv.Expression is MemberAccessExpressionSyntax innerMember)
                return innerMember.Name.Identifier.Text;
        }

        if (expr is ParenthesizedLambdaExpressionSyntax parenLambda)
        {
            if (parenLambda.Body is MemberAccessExpressionSyntax memberAccess)
                return memberAccess.Name.Identifier.Text;
        }

        return null;
    }

    private static string? ExtractMapFromExpression(ExpressionSyntax expr)
    {
        if (expr is SimpleLambdaExpressionSyntax configLambda)
        {
            if (configLambda.Body is InvocationExpressionSyntax inv
                && inv.Expression is MemberAccessExpressionSyntax memberAccess
                && memberAccess.Name.Identifier.Text == "MapFrom"
                && inv.ArgumentList.Arguments.Count >= 1)
            {
                var mapFromArg = inv.ArgumentList.Arguments[0].Expression;

                if (mapFromArg is SimpleLambdaExpressionSyntax sourceLambda)
                {
                    return sourceLambda.Body.ToString();
                }

                if (mapFromArg is ParenthesizedLambdaExpressionSyntax sourceParenLambda)
                {
                    return sourceParenLambda.Body.ToString();
                }

                if (mapFromArg is MemberAccessExpressionSyntax sourceMember)
                {
                    return sourceMember.ToString();
                }

                if (mapFromArg is LiteralExpressionSyntax)
                {
                    return mapFromArg.ToString();
                }
            }

            if (configLambda.Body is InvocationExpressionSyntax mapFromInv
                && mapFromInv.ArgumentList.Arguments.Count >= 1)
            {
                var arg = mapFromInv.ArgumentList.Arguments[0].Expression;
                if (arg is SimpleLambdaExpressionSyntax sl)
                    return sl.Body.ToString();
            }
        }

        return null;
    }

    private static bool IsIgnoreCall(ExpressionSyntax expr)
    {
        if (expr is SimpleLambdaExpressionSyntax lambda
            && lambda.Body is InvocationExpressionSyntax inv
            && inv.Expression is MemberAccessExpressionSyntax memberAccess
            && memberAccess.Name.Identifier.Text == "Ignore")
        {
            return true;
        }

        return false;
    }

    private static string? ExtractCallbackMethod(ExpressionSyntax expr, string callbackName)
    {
        if (expr is IdentifierNameSyntax idName)
            return idName.Identifier.Text;

        if (expr is MemberAccessExpressionSyntax memberAccess)
            return memberAccess.ToString();

        if (expr is SimpleLambdaExpressionSyntax simpleLambda)
        {
            var paramName = simpleLambda.Parameter.Identifier.Text;
            var body = simpleLambda.Body.ToString();
            return body.Replace(paramName, "source");
        }

        if (expr is ParenthesizedLambdaExpressionSyntax parenLambda)
        {
            var body = parenLambda.Body.ToString();
            var parameters = parenLambda.ParameterList.Parameters.ToArray();

            if (parameters.Length >= 1)
            {
                var srcName = parameters[0].Identifier.Text;
                body = body.Replace(srcName, "source");
            }

            if (parameters.Length >= 2 && callbackName == "AfterMap")
            {
                var dstName = parameters[1].Identifier.Text;
                body = body.Replace(dstName, "target");
            }

            return body;
        }

        if (expr is LiteralExpressionSyntax literal
            && literal.IsKind(SyntaxKind.StringLiteralExpression))
            return literal.Token.ValueText;

        return expr.ToString();
    }
}
