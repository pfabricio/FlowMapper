using FlowMapper.Compiler.Metadata;

namespace FlowMapper.Compiler.Optimization;

public interface IOptimizationPass
{
    string Name { get; }
    IOptimizedMetadataModel Execute(IMetadataModel metadata);
}
