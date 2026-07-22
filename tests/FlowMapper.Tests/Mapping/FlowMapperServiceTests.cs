using FlowMapper.Abstractions;
using FlowMapper.Mapping;
using FluentAssertions;
using Moq;

namespace FlowMapper.Tests.Mapping;

public class FlowMapperServiceTests
{
    [Fact]
    public void GetMapper_WithoutRegisteredMapper_ReturnsReflectionMapper()
    {
        var sp = Mock.Of<IServiceProvider>();
        var service = new FlowMapperService(sp);

        var mapper = service.GetMapper<Source, Dest>();

        mapper.Should().NotBeNull();
    }

    [Fact]
    public void Map_WithSimpleProperties_MapsByName()
    {
        var sp = Mock.Of<IServiceProvider>();
        var service = new FlowMapperService(sp);

        var dest = service.Map<Source, Dest>(new Source { Id = 1, Name = "Test" });

        dest.Should().NotBeNull();
        dest.Id.Should().Be(1);
        dest.Name.Should().Be("Test");
    }

    [Fact]
    public void Map_WithNullSource_ThrowsArgumentNullException()
    {
        var sp = Mock.Of<IServiceProvider>();
        var service = new FlowMapperService(sp);

        var act = () => service.Map<Source, Dest>(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GetMapper_ReturnsCachedInstance()
    {
        var sp = Mock.Of<IServiceProvider>();
        var service = new FlowMapperService(sp);

        var mapper1 = service.GetMapper<Source, Dest>();
        var mapper2 = service.GetMapper<Source, Dest>();

        mapper1.Should().BeSameAs(mapper2);
    }

    [Fact]
    public void Map_WithDifferentTypes_ReturnsCorrectType()
    {
        var sp = Mock.Of<IServiceProvider>();
        var service = new FlowMapperService(sp);

        var dest = service.Map<Source, Dest>(new Source { Id = 5, Name = "Alice" });

        dest.Should().BeOfType<Dest>();
        dest.Id.Should().Be(5);
        dest.Name.Should().Be("Alice");
    }

    public class Source
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }

    public class Dest
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }
}
