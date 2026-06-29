using FlowMapper.Abstractions;

namespace FlowMapper.DependencyInjection;

/// <summary>Runtime mapper dispatcher resolved from the DI container.
/// Provides type-safe mapping without requiring direct <c>IMapper&lt;,&gt;</c> dependencies.</summary>
public interface IFlowMapper
{
    /// <summary>Maps the specified source object to a new instance of <typeparamref name="TDestination"/>.</summary>
    /// <typeparam name="TSource">The source type.</typeparam>
    /// <typeparam name="TDestination">The destination type.</typeparam>
    /// <param name="source">The source object to map.</param>
    /// <returns>A new instance of <typeparamref name="TDestination"/> with values mapped from <paramref name="source"/>.</returns>
    TDestination Map<TSource, TDestination>(TSource source);

    /// <summary>Retrieves the registered <c>IMapper&lt;TSource, TDestination&gt;</c> from the DI container.</summary>
    /// <typeparam name="TSource">The source type.</typeparam>
    /// <typeparam name="TDestination">The destination type.</typeparam>
    /// <returns>The registered mapper instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no mapper is registered for the specified type pair.</exception>
    IMapper<TSource, TDestination> GetMapper<TSource, TDestination>();
}
