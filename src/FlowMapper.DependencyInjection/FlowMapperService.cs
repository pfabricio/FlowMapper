using FlowMapper.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace FlowMapper.DependencyInjection;

public class FlowMapperService : IFlowMapper
{
    private readonly IServiceProvider _serviceProvider;

    public FlowMapperService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public TDestination Map<TSource, TDestination>(TSource source)
    {
        var mapper = GetMapper<TSource, TDestination>();
        return mapper.Map(source);
    }

    public IMapper<TSource, TDestination> GetMapper<TSource, TDestination>()
    {
        var mapper = _serviceProvider.GetService<IMapper<TSource, TDestination>>();
        if (mapper != null) return mapper;

        var mappers = _serviceProvider.GetServices<IMapper<TSource, TDestination>>();
        mapper = mappers.FirstOrDefault();
        if (mapper != null) return mapper;

        throw new InvalidOperationException(
            $"No mapper registered for {typeof(TSource).Name} → {typeof(TDestination).Name}. " +
            "Ensure the source generator ran and the mapper class was discovered.");
    }
}
