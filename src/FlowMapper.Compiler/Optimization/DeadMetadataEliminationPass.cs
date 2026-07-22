using System.Diagnostics;
using FlowMapper.Compiler.Metadata;

namespace FlowMapper.Compiler.Optimization;

public sealed class DeadMetadataEliminationPass : IOptimizationPass
{
    public string Name => "Dead Metadata Elimination";

    public IOptimizedMetadataModel Execute(IMetadataModel metadata)
    {
        var sw = Stopwatch.StartNew();
        var removed = 0;

        var referenced = new HashSet<string>(StringComparer.Ordinal);
        foreach (var type in metadata.Types)
        {
            referenced.Add(type.Name);
            foreach (var member in type.Members)
                referenced.Add(member.TypeName);
            foreach (var iface in type.Interfaces)
                referenced.Add(iface);
            if (type.BaseType != null)
                referenced.Add(type.BaseType);
        }

        var optimized = metadata.Types
            .Where(t => referenced.Contains(t.Name))
            .Select(t => RemoveUnusedMembers(t, referenced, ref removed))
            .ToList()
            .AsReadOnly();

        sw.Stop();
        return new OptimizedMetadataModel(
            optimized,
            [new OptimizationReport(Name, "Removed dead types and members", removed, 0, sw.Elapsed)],
            metadata.Types.Count);
    }

    private static ITypeMetadata RemoveUnusedMembers(ITypeMetadata type, HashSet<string> referenced, ref int removed)
    {
        var referencedMembers = type.Members
            .Where(m => referenced.Contains(m.TypeName))
            .ToList()
            .AsReadOnly();

        removed += type.Members.Count - referencedMembers.Count;

        if (referencedMembers.Count == type.Members.Count)
            return type;

        return new TypeMetadata(
            type.Name,
            type.Namespace,
            type.BaseType,
            type.Interfaces,
            type.Constructors,
            referencedMembers,
            type.Tag);
    }
}
