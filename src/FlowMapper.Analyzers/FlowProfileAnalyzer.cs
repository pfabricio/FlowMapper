using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace FlowMapper.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class FlowProfileAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(DiagnosticDescriptors.FlowProfileInvalid);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
    }

    private static void AnalyzeNamedType(SymbolAnalysisContext context)
    {
        var typeSymbol = (INamedTypeSymbol)context.Symbol;

        var profileAttr = typeSymbol.GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.Name == "FlowProfileAttribute");

        if (profileAttr == null)
            return;

        if (typeSymbol.IsStatic)
        {
            var diag = Diagnostic.Create(
                DiagnosticDescriptors.FlowProfileInvalid,
                typeSymbol.Locations[0],
                typeSymbol.Name,
                "Static classes cannot be profiles");
            context.ReportDiagnostic(diag);
            return;
        }

        if (profileAttr.ConstructorArguments.Length == 0
            || string.IsNullOrWhiteSpace(profileAttr.ConstructorArguments[0].Value?.ToString()))
        {
            var diag = Diagnostic.Create(
                DiagnosticDescriptors.FlowProfileInvalid,
                typeSymbol.Locations[0],
                typeSymbol.Name,
                "Profile name cannot be empty");
            context.ReportDiagnostic(diag);
        }
    }
}
