namespace FlowMapper.Compiler.Compilation;

public interface IDependencyGraph
{
    void AddDependency(CompilationKey from, CompilationKey to);
    void Invalidate(CompilationKey key);
    bool IsValid(CompilationKey key);
    IReadOnlyCollection<CompilationKey> GetDependents(CompilationKey key);
    void Clear();
}
