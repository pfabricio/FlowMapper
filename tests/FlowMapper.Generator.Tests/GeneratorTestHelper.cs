#pragma warning disable CS0618

using System.Collections.Immutable;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace FlowMapper.Generator.Tests;

internal static class GeneratorTestHelper
{
    public static (ImmutableArray<Diagnostic> Diagnostics, string GeneratedSource) RunGenerator(
        string sourceCode)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode,
            CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp12));

        var metadataRefs = GetMetadataReferences();
        var compilation = CSharpCompilation.Create(
            "TestAssembly",
            new[] { syntaxTree },
            metadataRefs,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new FlowMapper.SourceGenerator.FlowMapperGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);
        driver.RunGeneratorsAndUpdateCompilation(
            compilation, out var updatedCompilation, out var diagnostics);

        var syntaxTrees = updatedCompilation.SyntaxTrees.ToList();
        var generatedSyntaxTree = syntaxTrees.FirstOrDefault(
            t => t.FilePath.EndsWith(".g.cs"));

        var generatedSource = generatedSyntaxTree?.GetText().ToString() ?? string.Empty;

        if (string.IsNullOrEmpty(generatedSource))
        {
            var allFiles = string.Join(", ", syntaxTrees.Select(t => t.FilePath));
            System.Console.Error.WriteLine($"No .g.cs file found. Available: {allFiles}");
            foreach (var d in diagnostics)
            {
                if (d.Severity == DiagnosticSeverity.Error)
                    System.Console.Error.WriteLine($"  Error: {d.Id} - {d.GetMessage()}");
            }
        }

        return (diagnostics, generatedSource);
    }

    public static List<MetadataReference> GetMetadataReferences()
    {
        var refs = new List<MetadataReference>();
        var assemblies = new[]
        {
            typeof(object).Assembly,
            typeof(Enumerable).Assembly,
            typeof(System.ComponentModel.EditorBrowsableAttribute).Assembly,
            typeof(Microsoft.CSharp.RuntimeBinder.RuntimeBinderException).Assembly,
            Assembly.Load("System.Runtime"),
            typeof(FlowMapper.Abstractions.MapAttribute<,>).Assembly,
            typeof(FlowMapper.Core.Flow).Assembly,
        };

        foreach (var asm in assemblies)
        {
            if (!string.IsNullOrEmpty(asm.Location))
            {
                refs.Add(MetadataReference.CreateFromFile(asm.Location));
            }
        }

        refs.Add(MetadataReference.CreateFromFile(
            Assembly.Load("netstandard").Location));
        refs.Add(MetadataReference.CreateFromFile(
            Assembly.Load("System.Collections").Location));
        refs.Add(MetadataReference.CreateFromFile(
            Assembly.Load("System.Linq").Location));

        return refs;
    }
}
