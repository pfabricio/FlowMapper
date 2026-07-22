using System.Linq.Expressions;

namespace FlowMapper.Core;

public class MappingExpression<TSource, TDestination>
{
    public string ProfileName { get; }
    public MappingPolicy Policy { get; }
    public List<ExplicitMapping> ExplicitMappings { get; } = new();
    public bool ReverseMapped { get; private set; }
    public bool UseConstructor { get; private set; }
    public bool DisableFlatten { get; private set; }

    public MappingExpression(string profileName, MappingPolicy policy)
    {
        ProfileName = profileName;
        Policy = policy;
    }

    public MappingExpression<TSource, TDestination> ForMember(
        Expression<Func<TDestination, object?>> destinationMember,
        Action<MemberOptions<TSource>> options)
    {
        var memberName = GetMemberName(destinationMember);
        var opts = new MemberOptions<TSource>();
        options(opts);

        ExplicitMappings.Add(new ExplicitMapping
        {
            DestinationProperty = memberName,
            SourceProperty = opts.SourceProperty ?? memberName,
            IsIgnored = opts.IsIgnored,
            MapFromExpression = opts.SourceExpression
        });

        return this;
    }

    public MappingExpression<TSource, TDestination> ForPath(
        Expression<Func<TDestination, object?>> destinationPath,
        Action<PathMemberOptions<TSource>> options)
    {
        var pathSegments = ExtractPathSegments(destinationPath);
        var opts = new PathMemberOptions<TSource>();
        options(opts);

        ExplicitMappings.Add(new ExplicitMapping
        {
            DestinationProperty = string.Join(".", pathSegments),
            SourceProperty = opts.SourceProperty ?? pathSegments.Last(),
            IsPathMapping = true,
            PathSegments = pathSegments,
            MapFromExpression = opts.SourceExpression
        });

        return this;
    }

    public MappingExpression<TSource, TDestination> ReverseMap()
    {
        ReverseMapped = true;
        return this;
    }

    public MappingExpression<TSource, TDestination> ConstructUsing(Expression<Func<TSource, TDestination>> constructor)
    {
        UseConstructor = true;
        return this;
    }

    public MappingExpression<TSource, TDestination> DisableFlattenMapping()
    {
        DisableFlatten = true;
        return this;
    }

    private static string GetMemberName(LambdaExpression expression)
    {
        return expression.Body switch
        {
            MemberExpression m => m.Member.Name,
            UnaryExpression { Operand: MemberExpression m } => m.Member.Name,
            _ => throw new ArgumentException("Invalid member expression")
        };
    }

    private static List<string> ExtractPathSegments(LambdaExpression expression)
    {
        var body = expression.Body is UnaryExpression u ? u.Operand : expression.Body;
        var segments = new List<string>();
        var current = body;
        while (current is MemberExpression m)
        {
            segments.Add(m.Member.Name);
            current = m.Expression;
        }
        segments.Reverse();
        return segments;
    }
}

public class MemberOptions<TSource>
{
    public string? SourceProperty { get; set; }
    public string? SourceExpression { get; set; }
    public bool IsIgnored { get; set; }

    public void MapFrom(string sourceProperty)
    {
        SourceProperty = sourceProperty;
    }

    public void MapFrom(Expression<Func<TSource, object?>> expression)
    {
        SourceExpression = expression.Body.ToString();
        if (expression.Body is MemberExpression m)
            SourceProperty = m.Member.Name;
    }

    public void Ignore()
    {
        IsIgnored = true;
    }
}

public class PathMemberOptions<TSource>
{
    public string? SourceProperty { get; set; }
    public string? SourceExpression { get; set; }

    public void MapFrom(string sourceProperty)
    {
        SourceProperty = sourceProperty;
    }

    public void MapFrom(Expression<Func<TSource, object?>> expression)
    {
        SourceExpression = expression.Body.ToString();
        if (expression.Body is MemberExpression m)
            SourceProperty = m.Member.Name;
    }
}
