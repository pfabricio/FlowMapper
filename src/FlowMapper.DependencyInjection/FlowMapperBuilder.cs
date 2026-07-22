using FlowMapper.Abstractions;
using FlowMapper.Core;
using FlowMapper.Providers.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace FlowMapper.DependencyInjection;

public class FlowMapperBuilder
{
    private readonly IServiceCollection _services;
    private readonly FlowMapperOptions _options;

    internal FlowMapperBuilder(IServiceCollection services)
    {
        _services = services;
        _options = new FlowMapperOptions();
    }

    public FlowMapperBuilder AddProvider<TProvider>(string connectionString)
        where TProvider : class, IDatabaseProvider
    {
        _services.AddSingleton<IDatabaseProvider>(sp =>
            ActivatorUtilities.CreateInstance<TProvider>(sp, connectionString));
        return this;
    }

    public FlowMapperBuilder AddProvider<TProvider>()
        where TProvider : class, IDatabaseProvider
    {
        _services.AddSingleton<IDatabaseProvider, TProvider>();
        return this;
    }

    public FlowMapperBuilder AddProfile<TProfile>()
        where TProfile : ProfileDefinition, new()
    {
        _services.AddSingleton<TProfile>();
        _services.AddSingleton(sp => (ProfileDefinition)sp.GetRequiredService<TProfile>());
        return this;
    }

    public FlowMapperBuilder ConfigureData(Action<DataOptions> configure)
    {
        configure(_options.Data);
        return this;
    }

    public FlowMapperBuilder ConfigureMapping(Action<MappingOptionsSection> configure)
    {
        configure(_options.Mapping);
        return this;
    }

    internal FlowMapperOptions GetOptions() => _options;
}
