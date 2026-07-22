using FlowMapper.Abstractions;
using FluentAssertions;

namespace FlowMapper.Tests.Abstractions;

public class MappingOptionsTests
{
    [Fact]
    public void DefaultSeparator_IsUnderscore()
    {
        var options = new MappingOptions();
        options.Separator.Should().Be("_");
    }

    [Fact]
    public void CanSetSeparator()
    {
        var options = new MappingOptions { Separator = "." };
        options.Separator.Should().Be(".");
    }
}
