namespace FlowMapper.Mapping.Pipeline.Middlewares;

public sealed class FlattenMiddleware : IMappingMiddleware
{
    public TDestination Map<TSource, TDestination>(
        TSource source, MappingDelegate<TSource, TDestination> next)
    {
        return next(source);
    }
}
