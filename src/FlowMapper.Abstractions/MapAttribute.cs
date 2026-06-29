using System.ComponentModel;

namespace FlowMapper.Abstractions;

/// <summary>Marks a partial class as an object mapper between <typeparamref name="TSource"/> and <typeparamref name="TDestination"/>.
/// The FlowMapper source generator reads this attribute and emits the mapping implementation at compile time.</summary>
/// <typeparam name="TSource">The source type to map from.</typeparam>
/// <typeparam name="TDestination">The destination type to map to.</typeparam>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
[EditorBrowsable(EditorBrowsableState.Never)]
[Obsolete("Use ProfileDefinition.CreateMap<T1, T2>() instead. " +
          "See https://flowmapper.dev/docs/migration-v1")]
public sealed class MapAttribute<TSource, TDestination> : Attribute
{
}
