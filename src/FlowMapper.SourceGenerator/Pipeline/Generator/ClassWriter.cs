using System.Text;

namespace FlowMapper.SourceGenerator.Pipeline.Generator;

public class ClassWriter : ICodeWriter
{
    public void Write(CodeWriterContext context, StringBuilder sb)
    {
        if (context.Flow.ProfileName != "Default")
        {
            sb.AppendLine($"[FlowProfile(\"{context.Flow.ProfileName}\")]");
        }

        sb.AppendLine($"public partial class {context.MapperName} : IMapper<{context.Flow.SourceType}, {context.Flow.DestinationType}>");
        sb.AppendLine("{");
    }
}