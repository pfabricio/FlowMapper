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

        // Get all required assemblies from the runtime
        var assemblyTypes = new (Assembly Assembly, bool Required)[]
        {
            (typeof(object).Assembly, true),                    // System.Runtime
            (typeof(Enumerable).Assembly, true),                // System.Linq
            (typeof(System.ComponentModel.EditorBrowsableAttribute).Assembly, true),
            (typeof(Microsoft.CSharp.RuntimeBinder.RuntimeBinderException).Assembly, false),
            (typeof(FlowMapper.Abstractions.MapAttribute<,>).Assembly, true),
            (typeof(FlowMapper.Core.Flow).Assembly, true),
            (typeof(FlowMapper.FullTextSearch.FtsProfileDefinition).Assembly, true),
        };

        foreach (var (asm, required) in assemblyTypes)
        {
            if (!string.IsNullOrEmpty(asm.Location))
                refs.Add(MetadataReference.CreateFromFile(asm.Location));
        }

        // Try loading additional assemblies by name (may fail on Linux but not critical)
        var optionalNames = new[] { "System.Runtime", "netstandard", "System.Collections", "System.Linq" };
        foreach (var name in optionalNames)
        {
            try
            {
                var asm = Assembly.Load(name);
                if (!string.IsNullOrEmpty(asm.Location))
                    refs.Add(MetadataReference.CreateFromFile(asm.Location));
            }
            catch
            {
                // Skip assemblies that can't be loaded (e.g. netstandard on modern .NET)
            }
        }

        return refs;
    }
}
