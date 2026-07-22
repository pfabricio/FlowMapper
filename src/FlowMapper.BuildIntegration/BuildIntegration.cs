using FlowMapper.Compiler.Compilation;
using FlowMapper.Compiler.Pipeline;
using FlowMapper.PluginSdk;

namespace FlowMapper.BuildIntegration;

public sealed class BuildIntegration : IBuildIntegration
{
    public CompilerPipelineResult Execute(BuildContext context)
    {
        var compilerContext = new CompilerContext
        {
            Cache = context.Cache,
            DependencyGraph = context.DependencyGraph,
            ProfileCount = 1
        };

        var pipeline = new CompilerPipeline(context.Stages);

        var result = pipeline.Execute(compilerContext);

        return result;
    }

    public static BuildContext CreateDefaultContext(
        string projectName,
        ICompilationCache? cache = null,
        IPluginRegistry? pluginRegistry = null)
    {
        var project = new ProjectMetadata(
            projectName,
            "bin/Debug/net8.0",
            projectName,
            projectName,
            "net8.0");

        var options = new CompilationOptions();

        var defaultStages = new List<ICompilerStage>
        {
            new MetadataStage(),
            new CompilationStage(new Compiler.Compiler(new FlowMapper.Core.FlowBuilder()))
        };

        return new BuildContext(
            project,
            options,
            cache ?? new CompilationCache(),
            new DependencyGraph(),
            defaultStages.AsReadOnly(),
            pluginRegistry);
    }
}
