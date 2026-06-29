#pragma warning disable CS1591

using System.Collections.Generic;

namespace FlowMapper.Core;

public class ProfileDefinition
{
    public string Name { get; set; } = string.Empty;
    public bool EnableFlatten { get; set; } = true;
    public bool PreferConstructor { get; set; }
    public bool StrictMapping { get; set; }

    internal List<object> Mappings { get; } = new();

    public MappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>()
    {
        var expression = new MappingExpression<TSource, TDestination>();
        Mappings.Add(expression);
        return expression;
    }
}
