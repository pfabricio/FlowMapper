using System.Linq;
using System.Text;
using FlowMapper.Core;

namespace FlowMapper.SourceGenerator.Pipeline.Generator;

public class ConstructorWriter : ICodeWriter
{
    public void Write(CodeWriterContext context, StringBuilder sb)
    {
        var flow = context.Flow;

        var constructorProps = flow.Properties
            .Where(p => p.Strategy == MappingStrategy.Constructor)
            .OrderBy(p => p.ConstructorParameterIndex)
            .ToList();

        var hasConstructor = constructorProps.Count > 0 || (!context.IsNested && flow.ConstructUsingMethod != null);

        if (!context.IsNested && flow.ConstructUsingMethod != null)
        {
            if (IsLambdaBody(flow.ConstructUsingMethod))
                sb.AppendLine($"        var target = {flow.ConstructUsingMethod};");
            else
                sb.AppendLine($"        var target = {flow.ConstructUsingMethod}(source);");
        }
        else if (hasConstructor)
        {
            sb.Append($"        var target = new {flow.DestinationType}(");
            for (int i = 0; i < constructorProps.Count; i++)
            {
                if (i > 0) sb.Append(", ");
                sb.Append($"source.{constructorProps[i].SourceProperty}");
            }
            sb.AppendLine(");");
        }
        else
        {
            sb.AppendLine($"        var target = new {flow.DestinationType}();");
        }
    }

    private static bool IsLambdaBody(string? methodOrExpression)
    {
        if (methodOrExpression == null)
            return false;
        if (methodOrExpression.Contains("=>"))
            return true;
        if (methodOrExpression.Contains("="))
            return true;
        if (methodOrExpression.StartsWith("new "))
            return true;
        if (methodOrExpression.Contains(";"))
            return true;
        return false;
    }
}