using FlowMapper.Compiler.Compilation;
using FlowMapper.Compiler.Pipeline;
using FlowMapper.PluginSdk;

namespace FlowMapper.BuildIntegration;

public sealed record BuildContext(
    ProjectMetadata Project,
    CompilationOptions Options,
    ICompilationCache Cache,
    IDependencyGraph DependencyGraph,
    IReadOnlyList<ICompilerStage> Stages,
    IPluginRegistry? PluginRegistry = null);

public sealed record ProjectMetadata(
    string Name,
    string OutputPath,
    string AssemblyName,
    string RootNamespace,
    string TargetFramework);

public sealed record CompilationOptions(
    bool EnableOptimizations = true,
    bool EnableSourceGeneration = true,
    bool EnableCaching = true,
    bool FailOnError = true);
