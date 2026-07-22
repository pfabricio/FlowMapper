using System.Diagnostics.CodeAnalysis;

namespace FlowMapper.Compiler.Compilation;

public interface ICompilationCache
{
    bool TryGet<T>(CompilationKey key, [MaybeNullWhen(false)] out T value);
    void Store<T>(CompilationKey key, T value);
    void Invalidate(CompilationKey key);
    void Clear();
    int Count { get; }
    IReadOnlyCollection<CompilationKey> Keys { get; }
}
