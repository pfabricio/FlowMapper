using FlowMapper.Compiler.Pipeline;

namespace FlowMapper.BuildIntegration;

public sealed class BuildDiagnosticsForwarder
{
    private readonly List<CompilerDiagnostic> _diagnostics = [];

    public IReadOnlyList<CompilerDiagnostic> Diagnostics => _diagnostics.AsReadOnly();

    public void Forward(CompilerPipelineResult result)
    {
        foreach (var diag in result.Diagnostics)
            ForwardDiagnostic(diag);
    }

    public void ForwardDiagnostic(CompilerDiagnostic diagnostic)
    {
        _diagnostics.Add(diagnostic);

        switch (diagnostic.Severity)
        {
            case CompilerDiagnosticSeverity.Error:
                Console.Error.WriteLine($"error FMPLR: {diagnostic.Message}");
                if (diagnostic.Detail != null)
                    Console.Error.WriteLine(diagnostic.Detail);
                break;

            case CompilerDiagnosticSeverity.Warning:
                Console.WriteLine($"warning FMPLR: {diagnostic.Message}");
                break;

            case CompilerDiagnosticSeverity.Info:
                Console.WriteLine($"info FMPLR: {diagnostic.Message}");
                break;
        }
    }

    public bool HasErrors => _diagnostics.Any(d => d.Severity == CompilerDiagnosticSeverity.Error);

    public void Clear() => _diagnostics.Clear();
}
