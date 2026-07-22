using FlowMapper.Compiler.Compilation;
using FlowMapper.Compiler.Metadata;
using FlowMapper.Compiler.Optimization;
using FlowMapper.Execution;
using FlowMapper.PluginSdk;

namespace FlowMapper.Compiler.Pipeline;

public sealed class CompilerContext
{
    public IMetadataModel? Metadata { get; set; }
    public IOptimizedMetadataModel? OptimizedMetadata { get; set; }
    public ICompilationCache? Cache { get; set; }
    public IDependencyGraph? DependencyGraph { get; set; }
    public IReadOnlyList<ICompilerStage>? PluginStages { get; set; }
    public List<CompilerDiagnostic> Diagnostics { get; } = [];
    public List<ExecutionArtifact> Artifacts { get; } = [];
    public List<ExecutionPlan> ExecutionPlans { get; } = [];
    public int ProfileCount { get; init; }

    public void AddDiagnostic(CompilerDiagnostic diagnostic)
    {
        Diagnostics.Add(diagnostic);
    }

    public void AddDiagnostics(IEnumerable<CompilerDiagnostic> diagnostics)
    {
        Diagnostics.AddRange(diagnostics);
    }

    public bool HasCriticalErrors =>
        Diagnostics.Any(d => d.Severity == CompilerDiagnosticSeverity.Error);
}
