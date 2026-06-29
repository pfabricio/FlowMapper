using FlowMapper.Abstractions;
using FlowMapper.Core;

namespace FlowMapper.UnitTests;

public class CoreModelTests
{
    [Fact]
    public void Flow_Has_Default_Properties()
    {
        var flow = new Flow();
        Assert.Equal("Default", flow.ProfileName);
        Assert.NotNull(flow.Properties);
        Assert.NotNull(flow.NestedFlows);
        Assert.NotNull(flow.ConstructorBindings);
        Assert.NotNull(flow.Policy);
    }

    [Fact]
    public void MappingPolicy_Defaults()
    {
        var policy = new MappingPolicy();
        Assert.Equal(StrictnessLevel.None, policy.Strictness);
        Assert.True(policy.EnableFlatten);
        Assert.False(policy.PreferConstructor);
    }

    [Fact]
    public void FlowSignature_CacheKey_Format()
    {
        var sig = new FlowSignature
        {
            ProfileName = "Api",
            SourceTypeId = "User",
            DestinationTypeId = "UserDto",
            PolicyHash = "ABC",
            PropertyHash = "DEF"
        };
        var key = sig.ToCacheKey();
        Assert.Equal("Api|User|UserDto|ABC|DEF", key);
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
    public void PropertyFlow_Supports_All_Strategies()
    {
        foreach (var strategy in Enum.GetValues<MappingStrategy>())
        {
            var pf = new PropertyFlow { Strategy = strategy };
            Assert.Equal(strategy, pf.Strategy);
        }
    }

    [Fact]
    public void FlattenPath_Roundtrips()
    {
        var path = new FlattenPath
        {
            FullPath = "Address.Street",
            Segments = new List<string> { "Address", "Street" },
            TargetProperty = "Street"
        };
        Assert.Equal("Address.Street", path.FullPath);
        Assert.Equal(2, path.Segments.Count);
    }

    [Fact]
    public void NestedFlow_Has_ChildFlow()
    {
        var nested = new NestedFlow
        {
            ParentProperty = "Address",
            ChildFlow = new Flow { SourceType = "Address", DestinationType = "AddressDto" }
        };
        Assert.NotNull(nested.ChildFlow);
        Assert.Equal("Address", nested.ParentProperty);
    }
}

public class ProfileDefinitionTests
{
    [Fact]
    public void CreateMap_Returns_MappingExpression()
    {
        var profile = new ProfileDefinition { Name = "Test" };
        var expr = profile.CreateMap<User, UserDto>();
        Assert.NotNull(expr);
    }

    private class User { public int Id { get; set; } }
    private class UserDto { public int Id { get; set; } }
}

public class MappingExpressionTests
{
    [Fact]
    public void Ignore_Does_Not_Throw()
    {
        var expr = new MappingExpression<Source, Dest>();
        var result = expr.Ignore(d => d.Name);
        Assert.Same(expr, result);
    }

    [Fact]
    public void UseConstructor_Returns_Self()
    {
        var expr = new MappingExpression<Source, Dest>();
        var result = expr.UseConstructor();
        Assert.Same(expr, result);
    }

    [Fact]
    public void DisableFlatten_Returns_Self()
    {
        var expr = new MappingExpression<Source, Dest>();
        var result = expr.DisableFlatten();
        Assert.Same(expr, result);
    }

    [Fact]
    public void ForMember_Returns_Self()
    {
        var expr = new MappingExpression<Source, Dest>();
        var result = expr.ForMember(d => d.Id, o => o.MapFrom("Identifier"));
        Assert.Same(expr, result);
    }

    private class Source { public int Id { get; set; } }
    private class Dest { public int Id { get; set; } public string Name { get; set; } = ""; }
}
