using FlowMapper.Execution.Artifacts;

namespace FlowMapper.Validation.Pipeline.Rules;

public sealed class NotNullRule : ValidationRule
{
    public override string Name => "NotNullCheck";

    public override ValidationResult Validate<T>(T target, IExecutionArtifact? artifact = null)
    {
        if (target == null)
            return Fail("Target cannot be null.", Name);

        return Success(Name);
    }
}
