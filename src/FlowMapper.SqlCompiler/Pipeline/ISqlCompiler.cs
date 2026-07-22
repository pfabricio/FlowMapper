using FlowMapper.Execution.Artifacts;

namespace FlowMapper.SqlCompiler.Pipeline;

public interface ISqlCompiler
{
    CompiledSql Compile(ISqlArtifact artifact);
    CompiledSql Compile(string sql, object? parameters = null);
}

public sealed record CompiledSql(
    string CommandText,
    IReadOnlyCollection<IParameterBinding> Parameters,
    Delegate? ParameterDelegate = null);
