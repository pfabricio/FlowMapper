using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FlowMapper.Analyzers;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MigrateToProfileCodeFixProvider))]
[Shared]
public class MigrateToProfileCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create("FM1004");

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
            .ConfigureAwait(false);
        if (root == null) return;

        var diagnostic = context.Diagnostics.First();
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        var classDecl = root.FindToken(diagnosticSpan.Start).Parent?
            .AncestorsAndSelf()
            .OfType<ClassDeclarationSyntax>()
            .FirstOrDefault();

        if (classDecl == null) return;

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Migrate to ProfileDefinition",
                createChangedSolution: ct => MigrateToProfileAsync(
                    context.Document, classDecl, ct),
                equivalenceKey: "MigrateToProfileDefinition"),
            diagnostic);
    }

    private static async Task<Solution> MigrateToProfileAsync(
        Document document,
        ClassDeclarationSyntax classDecl,
        CancellationToken ct)
    {
        var model = await document.GetSemanticModelAsync(ct).ConfigureAwait(false);
        if (model == null) return document.Project.Solution;

        var typeSymbol = model.GetDeclaredSymbol(classDecl, ct) as INamedTypeSymbol;
        if (typeSymbol == null) return document.Project.Solution;

        var mapperInterface = typeSymbol.AllInterfaces
            .FirstOrDefault(i => i.OriginalDefinition?.Name == "IMapper");

        if (mapperInterface?.TypeArguments.Length != 2)
            return document.Project.Solution;

        var srcType = mapperInterface.TypeArguments[0];
        var dstType = mapperInterface.TypeArguments[1];

        var profileName = $"{typeSymbol.Name.Replace("Mapper", "Profile")}Profile";

        var createMapInvocation =
            SyntaxFactory.ParseExpression($"CreateMap<{srcType.Name}, {dstType.Name}>()")
                as InvocationExpressionSyntax ?? SyntaxFactory.InvocationExpression(SyntaxFactory.IdentifierName("CreateMap"));

        var ctorBody = SyntaxFactory.Block(
            SyntaxFactory.SingletonList<StatementSyntax>(
                SyntaxFactory.ExpressionStatement(createMapInvocation)));

        var ctor = SyntaxFactory.ConstructorDeclaration(profileName)
            .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
            .WithBody(ctorBody);

        var profileClass = SyntaxFactory.ClassDeclaration(profileName)
            .WithModifiers(
                SyntaxFactory.TokenList(
                    SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
            .WithBaseList(
                SyntaxFactory.BaseList(
                    SyntaxFactory.SingletonSeparatedList<BaseTypeSyntax>(
                        SyntaxFactory.SimpleBaseType(
                            SyntaxFactory.IdentifierName("ProfileDefinition")))))
            .WithMembers(SyntaxFactory.SingletonList<MemberDeclarationSyntax>(ctor));

        var root = await document.GetSyntaxRootAsync(ct).ConfigureAwait(false);
        if (root == null) return document.Project.Solution;

        var newRoot = root.ReplaceNode(classDecl, profileClass);
        return document.WithSyntaxRoot(newRoot).Project.Solution;
    }
}
