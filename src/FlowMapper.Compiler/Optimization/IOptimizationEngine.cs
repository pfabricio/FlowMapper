using FlowMapper.Compiler.Metadata;

namespace FlowMapper.Compiler.Optimization;

public interface IOptimizationEngine
{
    IOptimizedMetadataModel Optimize(IMetadataModel metadata);
}
