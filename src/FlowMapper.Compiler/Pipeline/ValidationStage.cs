using System.Diagnostics;
using FlowMapper.Compiler.Metadata;
using FlowMapper.Validation.Pipeline;

namespace FlowMapper.Compiler.Pipeline;

public sealed class ValidationStage : ICompilerStage
{
    private readonly IValidationPipeline? _validationPipeline;

    public ValidationStage(IValidationPipeline? validationPipeline = null)
    {
        _validationPipeline = validationPipeline;
    }

    public string Name => "Validation";

    public CompilerStageResult Execute(CompilerContext context)
    {
        var sw = Stopwatch.StartNew();
        var diagnostics = new List<CompilerDiagnostic>();

        if (_validationPipeline == null || context.Metadata == null)
        {
            sw.Stop();
            diagnostics.Add(new CompilerDiagnostic(
                Name, "Validation skipped (no pipeline or metadata available)",
                CompilerDiagnosticSeverity.Info));
            return new CompilerStageResult(true, diagnostics.AsReadOnly(), sw.Elapsed);
        }

        try
        {
            var results = _validationPipeline.ValidateAll(context.Metadata.Types);
            var errors = results.Where(r => !r.IsValid).ToList();

            foreach (var result in results)
            {
                foreach (var error in result.Errors)
                    diagnostics.Add(new CompilerDiagnostic(Name, error, CompilerDiagnosticSeverity.Error, result.RuleName));

                foreach (var warning in result.Warnings)
                    diagnostics.Add(new CompilerDiagnostic(Name, warning, CompilerDiagnosticSeverity.Warning, result.RuleName));
            }

            if (errors.Count > 0)
            {
                sw.Stop();
                return new CompilerStageResult(false, diagnostics.AsReadOnly(), sw.Elapsed);
            }

            sw.Stop();
            diagnostics.Add(new CompilerDiagnostic(
                Name, $"Validated {context.Metadata.Types.Count} types, {results.Count} rules executed",
                CompilerDiagnosticSeverity.Info));
            return new CompilerStageResult(true, diagnostics.AsReadOnly(), sw.Elapsed);
        }
        catch (Exception ex)
        {
            sw.Stop();
            diagnostics.Add(new CompilerDiagnostic(
                Name, $"Validation failed: {ex.Message}",
                CompilerDiagnosticSeverity.Error, ex.StackTrace));
            return new CompilerStageResult(false, diagnostics.AsReadOnly(), sw.Elapsed);
        }
    }
}
