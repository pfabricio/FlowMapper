using FlowMapper.Abstractions;
using FlowMapper.Core;
using FluentAssertions;

namespace FlowMapper.Tests.Core;

public class FlowMapperOptionsTests
{
    [Fact]
    public void DefaultValues_AreCorrect()
    {
        var options = new FlowMapper.Core.FlowMapperOptions();

        options.Data.Should().NotBeNull();
        options.Data.DefaultTimeout.Should().BeNull();
        options.Data.Mapping.Should().NotBeNull();
        options.Data.Mapping.Separator.Should().Be("_");
        options.Data.Retry.Should().NotBeNull();
        options.Data.Retry.Enabled.Should().BeFalse();
        options.Data.Retry.MaxRetries.Should().Be(3);
        options.Data.Retry.InitialDelayMs.Should().Be(100);

        options.Mapping.Should().NotBeNull();
        options.Mapping.DefaultProfile.Should().Be("Default");
        options.Mapping.EnableFlatten.Should().BeTrue();
        options.Mapping.PreferConstructorMapping.Should().BeFalse();
        options.Mapping.Strictness.Should().Be(StrictnessLevel.None);
        options.Mapping.EnableCache.Should().BeTrue();
    }

    [Fact]
    public void CanSetDataOptions()
    {
        var options = new FlowMapper.Core.FlowMapperOptions
        {
            Data = new DataOptions
            {
                DefaultTimeout = 30,
                Mapping = new MappingOptions { Separator = "." },
                Retry = new RetryOptions { Enabled = true, MaxRetries = 5 }
            }
        };

        options.Data.DefaultTimeout.Should().Be(30);
        options.Data.Mapping.Separator.Should().Be(".");
        options.Data.Retry.Enabled.Should().BeTrue();
        options.Data.Retry.MaxRetries.Should().Be(5);
    }

    [Fact]
    public void CanSetMappingOptions()
    {
        var options = new FlowMapper.Core.FlowMapperOptions
        {
            Mapping = new MappingOptionsSection
            {
                DefaultProfile = "Custom",
                EnableFlatten = false,
                PreferConstructorMapping = true,
                Strictness = StrictnessLevel.Warning,
                EnableCache = false
            }
        };

        options.Mapping.DefaultProfile.Should().Be("Custom");
        options.Mapping.EnableFlatten.Should().BeFalse();
        options.Mapping.PreferConstructorMapping.Should().BeTrue();
        options.Mapping.Strictness.Should().Be(StrictnessLevel.Warning);
        options.Mapping.EnableCache.Should().BeFalse();
    }
}
