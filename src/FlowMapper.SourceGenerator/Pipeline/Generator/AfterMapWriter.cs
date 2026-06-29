using System.Text;

namespace FlowMapper.SourceGenerator.Pipeline.Generator;

public class AfterMapWriter : ICodeWriter
{
    public void Write(CodeWriterContext context, StringBuilder sb)
    {
        if (context.IsNested) return;

        var afterMap = context.Flow.AfterMapMethod;
        if (afterMap == null) return;

        if (IsLambdaBody(afterMap))
            sb.AppendLine($"        {afterMap};");
        else
            sb.AppendLine($"        {afterMap}(source, target);");
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