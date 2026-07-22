namespace FlowMapper.Mapping.Pipeline.Middlewares;

public sealed class NullPropagationMiddleware : IMappingMiddleware
{
    public TDestination Map<TSource, TDestination>(
        TSource source, MappingDelegate<TSource, TDestination> next)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        return next(source);
    }
}
