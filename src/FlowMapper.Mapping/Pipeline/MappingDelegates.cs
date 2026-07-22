namespace FlowMapper.Mapping.Pipeline;

public delegate TDestination MappingDelegate<TSource, TDestination>(TSource source);

public interface IMappingMiddleware
{
    TDestination Map<TSource, TDestination>(
        TSource source, MappingDelegate<TSource, TDestination> next);
}
