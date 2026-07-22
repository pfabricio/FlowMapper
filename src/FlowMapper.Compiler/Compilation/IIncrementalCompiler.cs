using FlowMapper.Core;
using FlowMapper.Execution;

namespace FlowMapper.Compiler.Compilation;

public interface IIncrementalCompiler
{
    CompilationResult Compile(IReadOnlyList<ProfileDefinition> profiles);
    CompilationResult CompileIncremental(IReadOnlyList<ProfileDefinition> profiles);
    void InvalidateCache(Type type);
    void InvalidateAll();
}

public enum CompilationStatus
{
    Cached,
    Compiled,
    Failed
}

public sealed record CompilationResult(
    CompilationStatus Status,
    IReadOnlyList<ExecutionArtifact> Artifacts,
    IReadOnlyList<string> Messages,
    TimeSpan Duration)
{
    public static CompilationResult Cached(IReadOnlyList<ExecutionArtifact> artifacts) =>
        new(CompilationStatus.Cached, artifacts, ["All artifacts served from cache"], TimeSpan.Zero);

    public static CompilationResult Compiled(IReadOnlyList<ExecutionArtifact> artifacts, TimeSpan duration) =>
        new(CompilationStatus.Compiled, artifacts, [$"Compiled {artifacts.Count} artifacts in {duration.TotalMilliseconds:F1}ms"], duration);

    public static CompilationResult Failed(string error, TimeSpan duration) =>
        new(CompilationStatus.Failed, [], [error], duration);
}
