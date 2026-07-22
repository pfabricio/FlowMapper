using System.Diagnostics;

namespace FlowMapper.Compiler.Pipeline;

public sealed class CompilationStage : ICompilerStage
{
    private readonly Compiler _compiler;

    public CompilationStage(Compiler compiler)
    {
        _compiler = compiler;
    }

    public string Name => "Compilation";

    public CompilerStageResult Execute(CompilerContext context)
    {
        var sw = Stopwatch.StartNew();
        var diagnostics = new List<CompilerDiagnostic>();

        try
        {
            var artifacts = _compiler.Compile([]);
            context.Artifacts.AddRange(artifacts);

            sw.Stop();
            diagnostics.Add(new CompilerDiagnostic(
                Name, $"Generated {artifacts.Count} execution artifacts in {sw.Elapsed.TotalMilliseconds:F1}ms",
                CompilerDiagnosticSeverity.Info));
            return new CompilerStageResult(true, diagnostics.AsReadOnly(), sw.Elapsed);
        }
        catch (Exception ex)
        {
            sw.Stop();
            diagnostics.Add(new CompilerDiagnostic(
                Name, $"Compilation failed: {ex.Message}",
                CompilerDiagnosticSeverity.Error, ex.StackTrace));
            return new CompilerStageResult(false, diagnostics.AsReadOnly(), sw.Elapsed);
        }
    }
}
