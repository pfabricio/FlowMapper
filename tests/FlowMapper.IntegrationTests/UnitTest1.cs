using FlowMapper.Abstractions;
using FlowMapper.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace FlowMapper.IntegrationTests;

public class DependencyInjectionTests
{
    [Fact]
    public void AddFlowMapper_Registers_IFlowMapper()
    {
        var services = new ServiceCollection();
        services.AddFlowMapper();
        var provider = services.BuildServiceProvider();

        var mapper = provider.GetService<IFlowMapper>();
        Assert.NotNull(mapper);
    }

    [Fact]
    public void AddFlowMapper_Registers_FlowMapperOptions()
    {
        var services = new ServiceCollection();
        services.AddFlowMapper(cfg => cfg.DefaultProfile = "Api");
        var provider = services.BuildServiceProvider();

        var options = provider.GetRequiredService<FlowMapperOptions>();
        Assert.Equal("Api", options.DefaultProfile);
    }

    [Fact]
    public void AddFlowMapper_DefaultOptions()
    {
        var services = new ServiceCollection();
        services.AddFlowMapper();
        var provider = services.BuildServiceProvider();

        var options = provider.GetRequiredService<FlowMapperOptions>();
        Assert.Equal("Default", options.DefaultProfile);
        Assert.True(options.EnableFlatten);
        Assert.Equal(StrictnessLevel.None, options.Strictness);
    }

    [Fact]
    public void FlowMapperService_Throws_When_Mapper_Not_Found()
    {
        var services = new ServiceCollection();
        services.AddFlowMapper();
        var provider = services.BuildServiceProvider();

        var mapper = provider.GetRequiredService<IFlowMapper>();
        var ex = Assert.Throws<InvalidOperationException>(() =>
            mapper.Map<UnmappedSource, UnmappedDest>(new UnmappedSource()));

        Assert.Contains("No mapper registered", ex.Message);
    }

    public class UnmappedSource { public int Id { get; set; } }
    public class UnmappedDest { public int Id { get; set; } }

    [Fact]
    public void FlowMapperService_Resolves_Registered_Mapper()
    {
        var services = new ServiceCollection();
        services.AddFlowMapper();
        services.AddTransient<IMapper<Source, Dest>, ManualMapper>();
        var provider = services.BuildServiceProvider();

        var mapper = provider.GetRequiredService<IFlowMapper>();
        var result = mapper.Map<Source, Dest>(new Source { Id = 5 });

        Assert.NotNull(result);
        Assert.Equal(5, result.Id);
    }

    public class Source { public int Id { get; set; } }
    public class Dest { public int Id { get; set; } }

    public class ManualMapper : IMapper<Source, Dest>
    {
        public Dest Map(Source source) => new() { Id = source.Id };
    }
}
