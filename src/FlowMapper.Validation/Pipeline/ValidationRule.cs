using FlowMapper.Execution.Artifacts;

namespace FlowMapper.Validation.Pipeline;

public interface IValidationRule
{
    string Name { get; }
    ValidationResult Validate<T>(T target, IExecutionArtifact? artifact = null);
}

public abstract class ValidationRule : IValidationRule
{
    public abstract string Name { get; }

    public abstract ValidationResult Validate<T>(T target, IExecutionArtifact? artifact = null);

    protected static ValidationResult Success(string? ruleName = null) =>
        ValidationResult.Success(ruleName);

    protected static ValidationResult Fail(string error, string? ruleName = null) =>
        ValidationResult.Fail(error, ruleName);
}
