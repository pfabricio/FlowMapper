using System;
using FlowMapper.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace FlowMapper.DependencyInjection;

/// <summary>Default implementation of <c>IFlowMapper</c>. Resolves <c>IMapper&lt;,&gt;</c> instances from
/// the <c>IServiceProvider</c>, which are registered during <c>AddFlowMapper()</c> startup.</summary>
public class FlowMapperService : IFlowMapper
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>Creates a new <c>FlowMapperService</c> with the given service provider.</summary>
    /// <param name="serviceProvider">The DI container to resolve mapper instances from.</param>
    public FlowMapperService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>Maps <paramref name="source"/> to <typeparamref name="TDestination"/> using the registered mapper.</summary>
    public TDestination Map<TSource, TDestination>(TSource source)
    {
        var mapper = GetMapper<TSource, TDestination>();
        return mapper.Map(source);
    }

    /// <summary>Resolves <c>IMapper&lt;TSource, TDestination&gt;</c> from the service provider.
    /// Mappers are registered by scanning assemblies for <c>IMapper&lt;,&gt;</c> implementations during startup.</summary>
    /// <exception cref="InvalidOperationException">Thrown when no mapper is registered for the type pair.</exception>
    public IMapper<TSource, TDestination> GetMapper<TSource, TDestination>()
    {
        var mapper = (IMapper<TSource, TDestination>?)_serviceProvider.GetService(
            typeof(IMapper<TSource, TDestination>));

        return mapper ?? throw new InvalidOperationException(
            $"No mapper registered for {typeof(TSource).Name} -> {typeof(TDestination).Name}. " +
            "Ensure AddFlowMapper() is called during startup and the mapper is generated.");
    }
}
