using FlowMapper.Core;
using Xunit;

namespace FlowMapper.UnitTests;

public class MappingExpressionTests
{
    [Fact]
    public void ForPath_CreatesPathMapping()
    {
        var expr = new MappingExpression<SourceDto, TargetDto>("Test", new MappingPolicy());

        expr.ForPath(d => d.Address.City, opt => opt.MapFrom("CityName"));

        Assert.Single(expr.ExplicitMappings);
        var mapping = expr.ExplicitMappings[0];
        Assert.True(mapping.IsPathMapping);
        Assert.Equal("Address.City", mapping.DestinationProperty);
        Assert.Contains("Address", mapping.PathSegments);
        Assert.Contains("City", mapping.PathSegments);
    }

    [Fact]
    public void ReverseMap_SetsFlag()
    {
        var expr = new MappingExpression<SourceDto, TargetDto>("Test", new MappingPolicy());

        expr.ReverseMap();

        Assert.True(expr.ReverseMapped);
    }

    [Fact]
    public void ForMember_CreatesExplicitMapping_WithString()
    {
        var expr = new MappingExpression<SourceDto, TargetDto>("Test", new MappingPolicy());

        expr.ForMember(d => d.FullName, opt => opt.MapFrom("Name"));

        Assert.Single(expr.ExplicitMappings);
        var mapping = expr.ExplicitMappings[0];
        Assert.Equal("FullName", mapping.DestinationProperty);
        Assert.Equal("Name", mapping.SourceProperty);
    }

    [Fact]
    public void ForMember_CreatesExplicitMapping_WithExpression()
    {
        var expr = new MappingExpression<SourceDto, TargetDto>("Test", new MappingPolicy());

        expr.ForMember(d => d.FullName, opt => opt.MapFrom(s => s.Name));

        Assert.Single(expr.ExplicitMappings);
        var mapping = expr.ExplicitMappings[0];
        Assert.Equal("FullName", mapping.DestinationProperty);
        Assert.Equal("Name", mapping.SourceProperty);
    }

    [Fact]
    public void ForMember_Ignore_SetsIgnored()
    {
        var expr = new MappingExpression<SourceDto, TargetDto>("Test", new MappingPolicy());

        expr.ForMember(d => d.FullName, opt => opt.Ignore());

        Assert.Single(expr.ExplicitMappings);
        Assert.True(expr.ExplicitMappings[0].IsIgnored);
    }
}

public class SourceDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string CityName { get; set; } = "";
}

public class TargetDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = "";
    public AddressDto Address { get; set; } = new();
}

public class AddressDto
{
    public string City { get; set; } = "";
}
