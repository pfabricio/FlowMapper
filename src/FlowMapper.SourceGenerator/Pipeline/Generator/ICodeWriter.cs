using System.Text;

namespace FlowMapper.SourceGenerator.Pipeline.Generator;

public interface ICodeWriter
{
    void Write(CodeWriterContext context, StringBuilder sb);
}