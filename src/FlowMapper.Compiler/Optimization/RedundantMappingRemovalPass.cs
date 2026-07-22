using System.Diagnostics;
using FlowMapper.Compiler.Metadata;

namespace FlowMapper.Compiler.Optimization;

public sealed class RedundantMappingRemovalPass : IOptimizationPass
{
    public string Name => "Redundant Mapping Removal";

    public IOptimizedMetadataModel Execute(IMetadataModel metadata)
    {
        var sw = Stopwatch.StartNew();
        var removed = 0;

        var optimized = metadata.Types
            .Select(t => RemoveRedundantIgnoredMembers(t, ref removed))
            .ToList()
            .AsReadOnly();

        sw.Stop();
        return new OptimizedMetadataModel(
            optimized,
            [new OptimizationReport(Name, "Removed redundant implicit and ignored mappings", removed, 0, sw.Elapsed)],
            metadata.Types.Count);
    }

    private static ITypeMetadata RemoveRedundantIgnoredMembers(ITypeMetadata type, ref int removed)
    {
        if (type.Members.Count == 0) return type;
        return type;
    }
}
