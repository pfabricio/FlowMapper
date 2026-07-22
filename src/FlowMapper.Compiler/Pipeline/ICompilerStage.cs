namespace FlowMapper.Compiler.Pipeline;

public interface ICompilerStage
{
    string Name { get; }
    CompilerStageResult Execute(CompilerContext context);
}
