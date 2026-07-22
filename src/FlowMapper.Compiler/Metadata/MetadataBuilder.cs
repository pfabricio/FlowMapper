using System.Collections.Concurrent;
using System.Reflection;

namespace FlowMapper.Compiler.Metadata;

public sealed class MetadataBuilder
{
    private readonly ConcurrentDictionary<Type, ITypeMetadata> _cache = new();

    public IMetadataModel Build(IEnumerable<Type> types)
    {
        var typeMetadatas = types.Select(BuildTypeMetadata).ToList().AsReadOnly();
        return new MetadataModel(typeMetadatas);
    }

    public ITypeMetadata BuildTypeMetadata(Type type)
    {
        return _cache.GetOrAdd(type, t =>
        {
            var constructors = t.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Select(c => new ConstructorMetadata(
                    c.GetParameters().Select(p =>
                        new ParameterMetadata(p.Name ?? "", p.ParameterType.Name) as IParameterMetadata
                    ).ToList().AsReadOnly(),
                    c.IsPublic
                ) as IConstructorMetadata)
                .ToList().AsReadOnly();

            var members = t.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Select(p => new MemberMetadata(
                    p.Name,
                    p.PropertyType.Name,
                    p.CanRead,
                    p.CanWrite,
                    p.GetMethod?.IsPublic == true || p.SetMethod?.IsPublic == true
                ) as IMemberMetadata)
                .ToList().AsReadOnly();

            return new TypeMetadata(
                t.Name,
                t.Namespace ?? "",
                t.BaseType?.Name,
                t.GetInterfaces().Select(i => i.Name).ToList().AsReadOnly(),
                constructors,
                members
            );
        });
    }

    public ITypeMetadata GetCached(Type type) =>
        _cache.TryGetValue(type, out var meta) ? meta : BuildTypeMetadata(type);

    public void Clear() => _cache.Clear();
}
