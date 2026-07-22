using FlowMapper.Execution.Artifacts;

namespace FlowMapper.Validation.Pipeline.Rules;

public sealed class TypeCompatibilityRule : ValidationRule
{
    public override string Name => "TypeCompatibilityCheck";

    public override ValidationResult Validate<T>(T target, IExecutionArtifact? artifact = null)
    {
        if (artifact == null)
            return Success(Name);

        if (artifact is IMappingArtifact mapping)
        {
            if (mapping.SourceType != typeof(T) && !typeof(T).IsSubclassOf(mapping.SourceType))
                return Fail(
                    $"Type '{typeof(T).Name}' is not compatible with source type '{mapping.SourceType.Name}'.",
                    Name);
        }

        return Success(Name);
    }
}
