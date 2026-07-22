using FlowMapper.Core;
using Xunit;

namespace FlowMapper.UnitTests;

public class FlowBuilderTests
{
    [Fact]
    public void Build_WithMatchingProperties_CreatesPropertyFlows()
    {
        var builder = new FlowBuilder();
        var flow = builder.Build(typeof(Source), typeof(Dest));

        Assert.Equal("SourceToDestMapper", flow.Name);
        Assert.NotEmpty(flow.Properties);
        Assert.Contains(flow.Properties, p => p.SourceProperty == "Id" && p.DestinationProperty == "Id");
        Assert.Contains(flow.Properties, p => p.SourceProperty == "Name" && p.DestinationProperty == "Name");
    }

    [Fact]
    public void Build_WithProfileAndReverseMap_CreatesFlow()
    {
        var profile = new TestReverseProfile();
        var builder = new FlowBuilder();

        var flow = builder.Build(typeof(Source), typeof(Dest), profile);

        Assert.NotNull(flow);
    }

    [Fact]
    public void Build_SamePair_ReturnsCachedResult()
    {
        var builder = new FlowBuilder();
        var flow1 = builder.Build(typeof(Source), typeof(Dest));
        var flow2 = builder.Build(typeof(Source), typeof(Dest));

        Assert.Same(flow1, flow2);
    }
}

public class TestReverseProfile : ProfileDefinition
{
    public TestReverseProfile()
    {
        CreateMap<Source, Dest>().ReverseMap();
    }
}
