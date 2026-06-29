using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using FlowMapper.SourceGenerator.Models;
using FlowMapper.SourceGenerator.Pipeline;
using FlowMapper.SourceGenerator.Pipeline.Generator;
using FlowMapper.SourceGenerator.Pipeline.Validator;

namespace FlowMapper.SourceGenerator;

[Generator]
public class FlowMapperGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var mapDefinitions = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: IsMapperClass,
                transform: GetSemanticModel)
            .Where(x => x is not null)
            .Select((x, _) => new[] { x! }.AsEnumerable());

        var profileDefinitions = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: IsProfileClass,
                transform: GetProfileDefinitions)
            .Where(x => x is not null)
            .Select((x, _) => x!);

        var allDefinitions = mapDefinitions.Collect()
            .Combine(profileDefinitions.Collect())
            .Select((tuple, _) =>
            {
                var list = new List<MapperDefinition>();
                foreach (var arr in tuple.Left)
                    list.AddRange(arr);
                foreach (var arr in tuple.Right)
                    list.AddRange(arr);
                return list.AsEnumerable();
            });

        var pipeline = allDefinitions
            .Select((items, _) => FlowPipeline.Execute(items.ToList()));

        context.RegisterSourceOutput(pipeline, EmitSource);
    }

    private static bool IsMapperClass(SyntaxNode node, CancellationToken ct)
    {
        return node is ClassDeclarationSyntax cls
            && cls.AttributeLists.Count > 0
            && cls.Modifiers.ToString().Contains("partial");
    }

    private static bool IsProfileClass(SyntaxNode node, CancellationToken ct)
    {
        return node is ClassDeclarationSyntax cls
            && cls.BaseList != null
            && cls.BaseList.Types.Any(t => t.Type.ToString().Contains("ProfileDefinition"));
    }

    private static MapperDefinition? GetSemanticModel(
        GeneratorSyntaxContext context,
        CancellationToken ct)
    {
        var classDecl = (ClassDeclarationSyntax)context.Node;

        var symbol = context.SemanticModel
            .GetDeclaredSymbol(classDecl, ct) as INamedTypeSymbol;

        if (symbol == null)
            return null;

        var attribute = symbol
            .GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.Name == "MapAttribute");

        if (attribute == null)
            return null;

        return MapperDefinitionFactory.Create(symbol, attribute);
    }

    private static IEnumerable<MapperDefinition>? GetProfileDefinitions(
        GeneratorSyntaxContext context,
        CancellationToken ct)
    {
        var classDecl = (ClassDeclarationSyntax)context.Node;

        var symbol = context.SemanticModel
            .GetDeclaredSymbol(classDecl, ct) as INamedTypeSymbol;

        if (symbol == null)
            return null;

        var baseType = symbol.BaseType;
        while (baseType != null)
        {
            if (baseType.Name == "ProfileDefinition")
            {
                return MapperDefinitionFactory.CreateFromProfile(symbol, context.SemanticModel.Compilation);
            }
            baseType = baseType.BaseType;
        }

        return null;
    }

    private static void EmitSource(
        SourceProductionContext context,
        FlowModel model)
    {
        foreach (var d in model.Diagnostics)
        {
            var descriptor = MapDescriptor(d);
            if (descriptor != null)
            {
                var fakeDescriptor = new DiagnosticDescriptor(
                    d.Id,
                    descriptor.Title,
                    d.Message,
                    descriptor.Category,
                    descriptor.DefaultSeverity,
                    descriptor.IsEnabledByDefault);
                context.ReportDiagnostic(Diagnostic.Create(fakeDescriptor, Location.None));
            }
        }

        var code = FlowCodeGenerator.Generate(model);
        context.AddSource($"{model.MapperName}.g.cs", code);
    }

    private static DiagnosticDescriptor? MapDescriptor(FlowDiagnosticResult d)
    {
        return d.Id switch
        {
            "FM0001" => FlowDiagnostics.MissingDestinationProperty,
            "FM0002" => FlowDiagnostics.TypeMismatch,
            "FM0003" => FlowDiagnostics.InvalidMapper,
            "FM0004" => FlowDiagnostics.IncompleteMapping,
            "FM0005" => FlowDiagnostics.MalformedMapAttribute,
            "FM0006" => FlowDiagnostics.CyclicReference,
            "FM0007" => FlowDiagnostics.ConstructorMismatch,
            "FM0008" => FlowDiagnostics.MissingConstructorBinding,
            "FM0009" => FlowDiagnostics.AmbiguousFlattenPath,
            "FM0010" => FlowDiagnostics.FlattenPathNotFound,
            "FM0011" => FlowDiagnostics.InvalidFlattenDepth,
            _ => null
        };
    }
}
