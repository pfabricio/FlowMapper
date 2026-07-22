using FlowMapper.Execution.Artifacts;

namespace FlowMapper.Validation.Pipeline;

public sealed class ValidationPipeline : IValidationPipeline
{
    private readonly IReadOnlyList<IValidationRule> _rules;
    private readonly IReadOnlyList<IValidationMiddleware> _middlewares;

    public ValidationPipeline(
        IEnumerable<IValidationRule>? rules = null,
        IEnumerable<IValidationMiddleware>? middlewares = null)
    {
        _rules = (rules as IReadOnlyList<IValidationRule> ?? rules?.ToList()) ?? [];
        _middlewares = (middlewares as IReadOnlyList<IValidationMiddleware> ?? middlewares?.ToList()) ?? [];
    }

    public ValidationResult Validate<T>(T target, IExecutionArtifact? artifact = null)
    {
        var results = _rules.Select(rule => rule.Validate(target, artifact)).ToList();

        var errors = results.Where(r => !r.IsValid).SelectMany(r => r.Errors).ToList();
        var warnings = results.SelectMany(r => r.Warnings).ToList();

        return new ValidationResult(
            errors.Count == 0,
            errors,
            warnings);
    }

    public IReadOnlyList<ValidationResult> ValidateAll<T>(
        IEnumerable<T> targets, IExecutionArtifact? artifact = null)
    {
        return targets.Select(t => Validate(t, artifact)).ToList();
    }
}
