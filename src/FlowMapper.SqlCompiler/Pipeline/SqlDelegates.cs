namespace FlowMapper.SqlCompiler.Pipeline;

public delegate CompiledSql SqlDelegate(string sql, object? parameters);

public interface ISqlMiddleware
{
    CompiledSql Process(string sql, object? parameters, SqlDelegate next);
}
