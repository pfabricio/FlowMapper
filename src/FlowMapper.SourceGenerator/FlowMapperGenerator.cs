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
    private const string MapAttributeName = "MapAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var mapperDefs = context.SyntaxProvider.CreateSyntaxProvider(
            predicate: IsCandidate,
            transform: TransformCandidate
        ).Where(m => m != null)!;

        var combined = mapperDefs.Collect();

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

    private static void GenerateCode(SourceProductionContext context, ImmutableArray<List<MapperDefinition>?> allDefs)
    {
        var definitions = new List<MapperDefinition>();
        foreach (var defList in allDefs)
        {
            if (defList != null)
                definitions.AddRange(defList);
        }

        if (definitions.Count == 0) return;

        var model = FlowPipeline.Execute(definitions);

        var source = FlowCodeGenerator.Generate(model);
        context.AddSource("FlowMapper_Mappers.g.cs", SourceText.From(source, Encoding.UTF8));
    }
}