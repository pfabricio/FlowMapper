namespace FlowMapper.Abstractions;

/// <summary>Defines a compile-time generated mapper that converts <typeparamref name="TSource"/> to <typeparamref name="TDestination"/>.</summary>
/// <typeparam name="TSource">The source type to map from.</typeparam>
/// <typeparam name="TDestination">The destination type to map to.</typeparam>
public interface IMapper<in TSource, out TDestination>
{
    /// <summary>Maps the specified source instance to a new instance of <typeparamref name="TDestination"/>.</summary>
    /// <param name="source">The source object to map from.</param>
    /// <returns>A new <typeparamref name="TDestination"/> instance with values copied from <paramref name="source"/>.</returns>
    TDestination Map(TSource source);
}
