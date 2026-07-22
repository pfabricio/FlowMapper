using FlowMapper.Compiler.Pipeline;

namespace FlowMapper.BuildIntegration;

public interface IBuildIntegration
{
    CompilerPipelineResult Execute(BuildContext context);
}
