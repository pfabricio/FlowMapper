#pragma warning disable CS1591

using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace FlowMapper.Core;

public class MappingExpression<TSource, TDestination>
{
    internal List<ExplicitMapping> ExplicitMappings { get; } = new();
    internal List<string> IgnoredProperties { get; } = new();
    internal bool PreferConstructor { get; private set; }
    internal bool EnableFlatten { get; private set; } = true;
    internal string? AfterMapMethod { get; set; }
    internal string? ConstructUsingMethod { get; set; }

    public MappingExpression<TSource, TDestination> ForMember(
        Expression<Func<TDestination, object?>> destMember,
        Action<MemberOptions> options)
    {
        if (destMember.Body is MemberExpression memberExpr)
        {
            var destName = memberExpr.Member.Name;
            var opts = new MemberOptions();
            options(opts);
            ExplicitMappings.Add(new ExplicitMapping
            {
                DestinationProperty = destName,
                SourceProperty = opts.SourceProperty ?? destName,
                IsIgnored = opts.IsIgnored,
                MapFromExpression = opts.SourceExpression
            });
        }
        return this;
    }

    public MappingExpression<TSource, TDestination> Ignore(Expression<Func<TDestination, object?>> destMember)
    {
        if (destMember.Body is MemberExpression memberExpr)
        {
            IgnoredProperties.Add(memberExpr.Member.Name);
        }
        return this;
    }

    public MappingExpression<TSource, TDestination> UseConstructor()
    {
        PreferConstructor = true;
        return this;
    }

    public MappingExpression<TSource, TDestination> DisableFlatten()
    {
        EnableFlatten = false;
        return this;
    }

    [Obsolete("Use AfterMap(Expression<Action<TSource, TDestination>> expression) instead")]
    public MappingExpression<TSource, TDestination> AfterMap(string methodName)
    {
        AfterMapMethod = methodName;
        return this;
    }

    public MappingExpression<TSource, TDestination> AfterMap(
        Expression<Action<TSource, TDestination>> expression)
    {
        AfterMapMethod = expression.Body.ToString();
        return this;
    }

    [Obsolete("Use ConstructUsing(Expression<Func<TSource, TDestination>> expression) instead")]
    public MappingExpression<TSource, TDestination> ConstructUsing(string methodName)
    {
        ConstructUsingMethod = methodName;
        return this;
    }

    public MappingExpression<TSource, TDestination> ConstructUsing(
        Expression<Func<TSource, TDestination>> expression)
    {
        ConstructUsingMethod = expression.Body.ToString();
        return this;
    }
}

public class MemberOptions
{
    public string? SourceProperty { get; set; }
    public bool IsIgnored { get; set; }
    public string? SourceExpression { get; set; }

    public void MapFrom(string sourceProperty)
    {
        SourceProperty = sourceProperty;
    }

    public void MapFrom(LambdaExpression expression)
    {
        SourceExpression = expression.Body.ToString();
    }

    public void Ignore()
    {
        IsIgnored = true;
    }
}

public class ExplicitMapping
{
    public string SourceProperty { get; set; } = string.Empty;
    public string DestinationProperty { get; set; } = string.Empty;
    public bool IsIgnored { get; set; }
    public string? MapFromExpression { get; set; }
}
