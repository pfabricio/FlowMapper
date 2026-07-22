using FlowMapper.Execution.Artifacts;

namespace FlowMapper.Validation.Pipeline.Rules;

public sealed class MappingCoverageRule : ValidationRule
{
    public override string Name => "MappingCoverageCheck";

    public override ValidationResult Validate<T>(T target, IExecutionArtifact? artifact = null)
    {
        if (artifact is not IMappingArtifact mapping)
            return Success(Name);

        if (mapping.MappingDelegate == null)
            return Fail("Mapping artifact has no compiled delegate.", Name);

        return Success(Name);
    }
}
