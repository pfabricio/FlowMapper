using System.Linq;
using System.Text;
using FlowMapper.Core;

namespace FlowMapper.SourceGenerator.Pipeline.Generator;

public class PropertyWriter : ICodeWriter
{
    public void Write(CodeWriterContext context, StringBuilder sb)
    {
        var flow = context.Flow;

        var assignProps = flow.Properties
            .Where(p => p.Strategy is MappingStrategy.Direct or MappingStrategy.Flatten)
            .ToList();

        var hasAssignments = assignProps.Count > 0 || flow.NestedFlows.Count > 0;

        if (!hasAssignments) return;

        foreach (var prop in assignProps)
        {
            if (prop.MapFromExpression != null)
            {
                var expr = NormalizeExpression(prop.MapFromExpression);
                sb.AppendLine($"        target.{prop.DestinationProperty} = {expr};");
            }
            else
            {
                var sourceAccess = prop.Strategy == MappingStrategy.Flatten
                    ? $"source.{prop.SourcePath}"
                    : $"source.{prop.SourceProperty}";
                sb.AppendLine($"        target.{prop.DestinationProperty} = {sourceAccess};");
            }
        }
    }

    private static string NormalizeExpression(string expr)
    {
        return expr.Replace("$", "").Replace("\"", "\"");
    }
}