using System.Linq;
using System.Text;
using FlowMapper.SourceGenerator.Models;

namespace FlowMapper.SourceGenerator.Pipeline.Generator;

public class NestedWriter : ICodeWriter
{
    private static readonly ConstructorWriter _ctorWriter = new();
    private static readonly PropertyWriter _propWriter = new();

    public void Write(CodeWriterContext context, StringBuilder sb)
    {
        var flow = context.Flow;

        if (!context.IsNested)
        {
            foreach (var nested in flow.NestedFlows)
            {
                var methodName = GetNestedMethodName(nested.ParentProperty);
                sb.AppendLine($"        target.{nested.ParentProperty} = {methodName}(source.{nested.ParentProperty});");
            }
        }

        foreach (var nested in flow.NestedFlows)
        {
            WriteNestedMethod(nested, sb);
        }
    }

    private void WriteNestedMethod(NestedFlowModel nested, StringBuilder sb)
    {
        var childFlow = nested.ChildFlow;
        var methodName = GetNestedMethodName(nested.ParentProperty);

        sb.AppendLine();
        sb.AppendLine($"    private {childFlow.DestinationType} {methodName}({childFlow.SourceType} source)");
        sb.AppendLine("    {");

        var childContext = new CodeWriterContext
        {
            Flow = childFlow,
            IsNested = true,
            MethodName = methodName
        };

        _ctorWriter.Write(childContext, sb);

        var assignProps = childFlow.Properties
            .Where(p => p.ConstructorParameterIndex < 0)
            .ToList();

        var hasAssignments = assignProps.Count > 0 || childFlow.NestedFlows.Count > 0;
        if (hasAssignments)
        {
            _propWriter.Write(childContext, sb);

            foreach (var childNested in childFlow.NestedFlows)
            {
                var childMethod = GetNestedMethodName(childNested.ParentProperty);
                sb.AppendLine($"        target.{childNested.ParentProperty} = {childMethod}(source.{childNested.ParentProperty});");
            }
        }

        sb.AppendLine("        return target;");
        sb.AppendLine("    }");

        foreach (var childNested in childFlow.NestedFlows)
        {
            WriteNestedMethod(childNested, sb);
        }
    }

    private static string GetNestedMethodName(string propertyName)
    {
        return $"Map{propertyName}";
    }
}