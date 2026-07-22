using FlowMapper.Core;
using FlowMapper.Execution;

namespace FlowMapper.Compiler;

public interface ICompiler
{
    IReadOnlyList<ExecutionArtifact> Compile(IReadOnlyList<ProfileDefinition> profiles);
}
