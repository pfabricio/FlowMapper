using System.Diagnostics;
using FlowMapper.Compiler.Metadata;

namespace FlowMapper.Compiler.Pipeline;

public sealed class MetadataStage : ICompilerStage
{
    private readonly MetadataBuilder _builder;

    public MetadataStage(MetadataBuilder? builder = null)
    {
        _builder = builder ?? new MetadataBuilder();
    }

    public string Name => "Metadata";

    public CompilerStageResult Execute(CompilerContext context)
    {
        var sw = Stopwatch.StartNew();
        var diagnostics = new List<CompilerDiagnostic>();

        if (context.Metadata != null)
        {
            sw.Stop();
            return new CompilerStageResult(true, diagnostics.AsReadOnly(), sw.Elapsed);
        }

        try
        {
            var types = context.Artifacts
                .SelectMany(a => new[] { a.SourceType, a.DestinationType })
                .Distinct()
                .ToList();

            if (types.Count == 0)
            {
                sw.Stop();
                return new CompilerStageResult(true, diagnostics.AsReadOnly(), sw.Elapsed);
            }

            var metadata = _builder.Build(types);
            context.Metadata = metadata;

            diagnostics.Add(new CompilerDiagnostic(
                Name, $"Built metadata for {types.Count} types",
                CompilerDiagnosticSeverity.Info));

            sw.Stop();
            return new CompilerStageResult(true, diagnostics.AsReadOnly(), sw.Elapsed);
        }
        catch (Exception ex)
        {
            sw.Stop();
            diagnostics.Add(new CompilerDiagnostic(
                Name, $"Failed to build metadata: {ex.Message}",
                CompilerDiagnosticSeverity.Error, ex.StackTrace));
            return new CompilerStageResult(false, diagnostics.AsReadOnly(), sw.Elapsed);
        }
    }
}
