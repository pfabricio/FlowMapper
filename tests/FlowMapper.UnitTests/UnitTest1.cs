using Xunit;
using FlowMapper.Core;
using FlowMapper.Abstractions;

namespace FlowMapper.UnitTests;

public class CoreModelTests
{
    [Fact]
    public void Flow_Has_Default_Properties()
    {
        var sig = new FlowSignature { SourceType = typeof(string), DestinationType = typeof(int) };
        var flow = new Flow { Signature = sig, Name = "Test" };
        Assert.NotNull(flow.Properties);
        Assert.NotNull(flow.NestedFlows);
        Assert.NotNull(flow.Signature);
    }

    [Fact]
    public void MappingPolicy_Defaults()
    {
        var policy = new MappingPolicy();
        Assert.Equal(StrictnessLevel.Warning, policy.Strictness);
        Assert.True(policy.EnableFlatten);
        Assert.False(policy.PreferConstructor);
    }

    [Fact]
    public void FlowSignature_Equals_Works()
    {
        var sig1 = new FlowSignature { SourceType = typeof(string), DestinationType = typeof(int) };
        var sig2 = new FlowSignature { SourceType = typeof(string), DestinationType = typeof(int) };
        Assert.Equal(sig1, sig2);
    }

    [Fact]
    public void ConstructorBinding_Has_Index()
    {
        var binding = new ConstructorBinding
        {
            ParameterName = "id",
            SourceProperty = "Id",
            Index = 0
        };
        Assert.Equal(0, binding.Index);
        Assert.Equal("id", binding.ParameterName);
        Assert.Equal("Id", binding.SourceProperty);
    }

    [Fact]
    public void PropertyFlow_Roundtrips()
    {
        var pf = new PropertyFlow
        {
            SourceProperty = "Name",
            DestinationProperty = "FullName",
            SourceType = typeof(string),
            DestinationType = typeof(string)
        };
        Assert.Equal("Name", pf.SourceProperty);
        Assert.Equal("FullName", pf.DestinationProperty);
    }

    [Fact]
    public void FlattenPath_Roundtrips()
    {
        var path = new FlattenPath
        {
            ColumnName = "Address_Street",
            PathSegments = new List<string> { "Address", "Street" }
        };
        Assert.Equal("Address.Street", path.PropertyPath);
        Assert.Equal(2, path.PathSegments.Count);
    }

    [Fact]
    public void NestedFlow_Has_ChildFlow()
    {
        var sig = new FlowSignature { SourceType = typeof(string), DestinationType = typeof(int) };
        var nested = new NestedFlow
        {
            ParentProperty = "Address",
            ChildFlow = new Flow { Signature = sig, Name = "Child" }
        };
        Assert.NotNull(nested.ChildFlow);
        Assert.Equal("Address", nested.ParentProperty);
    }

    [Fact]
    public void ExplicitMapping_Roundtrips()
    {
        var m = new ExplicitMapping
        {
            SourceProperty = "Name",
            DestinationProperty = "FullName",
            IsPathMapping = true,
            PathSegments = new List<string> { "Nested", "Name" }
        };
        Assert.True(m.IsPathMapping);
        Assert.Equal(2, m.PathSegments.Count);
    }
}

