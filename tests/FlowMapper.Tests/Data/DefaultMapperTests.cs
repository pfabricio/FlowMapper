using FlowMapper.Data;
using FlowMapper.Data.Mapping;
using FluentAssertions;

namespace FlowMapper.Tests.Data;

public class DefaultMapperTests
{
    [Fact]
    public void Constructor_WithNullNamingStrategy_DoesNotThrow()
    {
        var act = () => new DefaultMapper(null);
        act.Should().NotThrow();
    }

    [Fact]
    public void Constructor_WithDefaultNamingStrategy_DoesNotThrow()
    {
        var act = () => new DefaultMapper(new DefaultNamingStrategy());
        act.Should().NotThrow();
    }
}
