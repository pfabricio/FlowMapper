using System.Data;
using System.Reflection;
using FlowMapper.Execution;
using FlowMapper.Execution.Artifacts;
using FlowMapper.Materializer.Pipeline;

namespace FlowMapper.Materializer;

public class Materializer : IMaterializer
{
    private readonly MaterializationPipeline _pipeline;
    private readonly string _separator;

    public Materializer() : this(new MaterializationPipeline(), "_") { }

    public Materializer(MaterializationPipeline pipeline, string? separator = "_")
    {
        _pipeline = pipeline;
        _separator = separator ?? "_";
    }

    public T Materialize<T>(IDataReader reader, MaterializationPlan plan)
    {
        var artifact = ConvertToArtifact<T>(plan);
        return _pipeline.Materialize<T>(reader, artifact);
    }

    public IEnumerable<T> MaterializeAll<T>(IDataReader reader, MaterializationPlan plan)
    {
        var artifact = ConvertToArtifact<T>(plan);
        return _pipeline.MaterializeAll<T>(reader, artifact);
    }

    public T Materialize<T>(IDataReader reader, IMaterializationArtifact artifact)
    {
        return _pipeline.Materialize<T>(reader, artifact);
    }

    public IEnumerable<T> MaterializeAll<T>(IDataReader reader, IMaterializationArtifact artifact)
    {
        return _pipeline.MaterializeAll<T>(reader, artifact);
    }

    public MaterializationPlan BuildPlan<T>()
    {
        return BuildPlanFlat<T>();
    }

    public static MaterializationPlan BuildPlanFlat<T>()
    {
        var plan = new MaterializationPlan { TargetType = typeof(T) };
        BuildPlanRecursive(typeof(T), plan.Bindings, "");
        return plan;
    }

    private static void BuildPlanRecursive(Type type, List<MaterializationBinding> bindings, string prefix)
    {
        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite))
        {
            if (IsComplexType(prop.PropertyType))
            {
                var nestedPrefix = string.IsNullOrEmpty(prefix) ? prop.Name : $"{prefix}_{prop.Name}";
                BuildPlanRecursive(prop.PropertyType, bindings, nestedPrefix);
            }
            else
            {
                var columnName = string.IsNullOrEmpty(prefix) ? prop.Name : $"{prefix}_{prop.Name}";
                bindings.Add(new MaterializationBinding
                {
                    ColumnName = columnName,
                    PropertyName = columnName,
                    PropertyType = prop.PropertyType
                });
            }
        }
    }

    private static bool IsComplexType(Type type)
    {
        return type.IsClass
            && type != typeof(string)
            && !type.IsValueType
            && !typeof(System.Collections.IEnumerable).IsAssignableFrom(type);
    }

    private IMaterializationArtifact ConvertToArtifact<T>(MaterializationPlan plan)
    {
        var bindings = plan.Bindings.Select(b => new ColumnBinding(
            b.ColumnName,
            b.PropertyName,
            b.PropertyType,
            Converter: null,
            IsNullable: Nullable.GetUnderlyingType(b.PropertyType) != null || !b.PropertyType.IsValueType
        )).ToArray();

        return new MaterializationArtifact(
            Name: $"Materialize_{typeof(T).Name}",
            Version: new Version(2, 0),
            TargetType: typeof(T),
            Separator: _separator,
            ConstructorDelegate: null,
            ColumnBindings: bindings,
            MaterializationDelegate: null
        );
    }
}
