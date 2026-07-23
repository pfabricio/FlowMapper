using System.Text;
using FlowMapper.SourceGenerator.Models;

namespace FlowMapper.SourceGenerator.Pipeline.Generator;

public static class FlowCodeGenerator
{
    private static readonly UsingWriter _usingWriter = new();
    private static readonly NamespaceWriter _nsWriter = new();
    private static readonly ClassWriter _classWriter = new();
    private static readonly ConstructorWriter _ctorWriter = new();
    private static readonly PropertyWriter _propWriter = new();
    private static readonly NestedWriter _nestedWriter = new();
    private static readonly AfterMapWriter _afterMapWriter = new();

    public static string Generate(FlowModel model)
    {
        var sb = new StringBuilder();

        _usingWriter.Write(new CodeWriterContext(), sb);

        string? profileNs = null;
        foreach (var flow in model.Flows)
        {
            if (flow.ProfileName != "Default")
            {
                profileNs = flow.ProfileName;
                break;
            }
        }

        var nsFlow = new FlowDescriptor
        {
            ProfileName = profileNs ?? "Default"
        };
        var nsCtx = new CodeWriterContext { Flow = nsFlow };
        _nsWriter.Write(nsCtx, sb);

        foreach (var flow in model.Flows)
        {
            var ctx = new CodeWriterContext
            {
                Flow = flow,
                MapperName = model.MapperName,
                IsNested = false
            };

            _classWriter.Write(ctx, sb);

            sb.AppendLine($"    public {flow.DestinationType} Map({flow.SourceType} source)");
            sb.AppendLine("    {");
            _ctorWriter.Write(ctx, sb);
            _propWriter.Write(ctx, sb);
            _nestedWriter.Write(ctx, sb);
            _afterMapWriter.Write(ctx, sb);
            sb.AppendLine("        return target;");
            sb.AppendLine("    }");

            var nestedCtx = new CodeWriterContext
            {
                Flow = flow,
                IsNested = false
            };
            _nestedWriter.Write(nestedCtx, sb);

            sb.AppendLine("}");
        }

        return sb.ToString();
    }
}