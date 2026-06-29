using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace FlowMapper.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class UnmappedPropertyAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(DiagnosticDescriptors.UnmappedDestinationProperty);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeClass, SyntaxKind.ClassDeclaration);
    }

    private static void AnalyzeClass(SyntaxNodeAnalysisContext context)
    {
        var classDecl = (ClassDeclarationSyntax)context.Node;

        var hasMapAttribute = classDecl.AttributeLists
            .SelectMany(a => a.Attributes)
            .Any(a => a.Name.ToString().Contains("Map"));

        if (!hasMapAttribute)
            return;

        var model = context.SemanticModel;
        var typeSymbol = model.GetDeclaredSymbol(classDecl) as INamedTypeSymbol;
        if (typeSymbol == null)
            return;

        var mapAttr = typeSymbol.GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.Name == "MapAttribute");

        if (mapAttr?.AttributeClass?.TypeArguments.Length != 2)
            return;

        var destType = mapAttr.AttributeClass.TypeArguments[1] as INamedTypeSymbol;
        var sourceType = mapAttr.AttributeClass.TypeArguments[0] as INamedTypeSymbol;

        if (destType == null || sourceType == null)
            return;

        var sourceNames = new HashSet<string>(
            sourceType.GetMembers()
                .OfType<IPropertySymbol>()
                .Select(p => p.Name));

        foreach (var destProp in destType.GetMembers().OfType<IPropertySymbol>())
        {
            if (!sourceNames.Contains(destProp.Name))
            {
                var propDecl = destProp.DeclaringSyntaxReferences
                    .FirstOrDefault()?.GetSyntax() as PropertyDeclarationSyntax;

                if (propDecl != null)
                {
                    var diag = Diagnostic.Create(
                        DiagnosticDescriptors.UnmappedDestinationProperty,
                        propDecl.Identifier.GetLocation(),
                        destType.Name,
                        destProp.Name);
                    context.ReportDiagnostic(diag);
                }
            }
        }
    }
}
