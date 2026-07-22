using FlowMapper.Execution.Artifacts;

namespace FlowMapper.Validation.Pipeline;

public delegate ValidationResult ValidationRuleDelegate<in T>(T target, IExecutionArtifact? artifact);

public interface IValidationMiddleware
{
    ValidationResult Validate<T>(T target, IExecutionArtifact? artifact, ValidationRuleDelegate<T> next);
}
