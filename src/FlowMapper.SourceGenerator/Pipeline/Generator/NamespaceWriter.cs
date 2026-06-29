using System.Text;

namespace FlowMapper.SourceGenerator.Pipeline.Generator;

public class NamespaceWriter : ICodeWriter
{
    public void Write(CodeWriterContext context, StringBuilder sb)
    {
        var profileNs = context.Flow.ProfileName != "Default"
            ? context.Flow.ProfileName
            : null;

        if (profileNs != null)
        {
            sb.AppendLine($"namespace FlowMapper.SourceGenerator.Profiles.{profileNs};");
        }
        else
        {
            sb.AppendLine("namespace FlowMapper.SourceGenerator;");
        }

        sb.AppendLine();
    }
}