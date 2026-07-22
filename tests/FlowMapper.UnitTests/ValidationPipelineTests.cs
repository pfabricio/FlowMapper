using FlowMapper.Execution.Artifacts;
using FlowMapper.Validation.Pipeline;
using FlowMapper.Validation.Pipeline.Rules;
using Xunit;

namespace FlowMapper.UnitTests;

public class ValidationPipelineTests
{
    [Fact]
    public void Validate_NullTarget_ReturnsErrors()
    {
        var pipeline = new ValidationPipeline(rules: [new NotNullRule()]);

        var result = pipeline.Validate<object>(null!);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("cannot be null"));
    }

    [Fact]
    public void Validate_ValidTarget_ReturnsSuccess()
    {
        var pipeline = new ValidationPipeline(rules: [new NotNullRule()]);

        var result = pipeline.Validate("valid");

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_TypeMismatch_ReturnsErrors()
    {
        var artifact = new MappingArtifact(
            "Test", new Version(2, 0),
            typeof(string), typeof(int),
            null, null, null, null);

        var pipeline = new ValidationPipeline(rules: [new TypeCompatibilityRule()]);

        var result = pipeline.Validate(42, artifact);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("not compatible"));
    }

    [Fact]
    public void Validate_MappingCoverage_MissingDelegate_ReturnsErrors()
    {
        var artifact = new MappingArtifact(
            "Test", new Version(2, 0),
            typeof(string), typeof(int),
            null, null, null, null);

        var pipeline = new ValidationPipeline(rules: [new MappingCoverageRule()]);

        var result = pipeline.Validate("hello", artifact);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("no compiled delegate"));
    }

    [Fact]
    public void ValidateAll_MultipleItems_ReturnsAllResults()
    {
        var pipeline = new ValidationPipeline(rules: [new NotNullRule()]);

        var results = pipeline.ValidateAll([null, "valid", null]);

        Assert.Equal(3, results.Count);
        Assert.False(results[0].IsValid);
        Assert.True(results[1].IsValid);
        Assert.False(results[2].IsValid);
    }

    [Fact]
    public void TypeCompatibilityRule_WithNullArtifact_Succeeds()
    {
        var rule = new TypeCompatibilityRule();

        var result = rule.Validate("hello");

        Assert.True(result.IsValid);
    }

    [Fact]
    public void NotNullRule_Name_IsCorrect()
    {
        Assert.Equal("NotNullCheck", new NotNullRule().Name);
    }
}
