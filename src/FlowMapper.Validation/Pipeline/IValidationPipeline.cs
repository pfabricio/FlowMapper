using FlowMapper.Execution.Artifacts;

namespace FlowMapper.Validation.Pipeline;

public interface IValidationPipeline
{
    ValidationResult Validate<T>(T target, IExecutionArtifact? artifact = null);
    IReadOnlyList<ValidationResult> ValidateAll<T>(IEnumerable<T> targets, IExecutionArtifact? artifact = null);
}

public sealed record ValidationResult(
    bool IsValid,
    IReadOnlyCollection<string> Errors,
    IReadOnlyCollection<string> Warnings,
    string? RuleName = null)
{
    public static ValidationResult Success(string? ruleName = null) =>
        new(true, [], [], ruleName);

    public static ValidationResult Fail(string error, string? ruleName = null) =>
        new(false, [error], [], ruleName);
}
