using System.ComponentModel;

namespace FlowMapper.Abstractions;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
[EditorBrowsable(EditorBrowsableState.Never)]
[Obsolete("Use ProfileDefinition.CreateMap<T1, T2>() instead. " +
          "See https://flowmapper.dev/docs/migration-v1")]
public sealed class MapAttribute<TSource, TDestination> : Attribute
{
}
