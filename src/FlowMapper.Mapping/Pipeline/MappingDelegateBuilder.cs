using System.Linq.Expressions;
using System.Reflection;
using FlowMapper.Core;
using FlowMapper.Execution.Artifacts;

namespace FlowMapper.Mapping.Pipeline;

public class MappingDelegateBuilder
{
    public virtual MappingDelegate<TSource, TDestination> BuildDelegate<TSource, TDestination>(
        IMappingArtifact artifact)
    {
        return BuildFromFlow<TSource, TDestination>(artifact);
    }

    public MappingDelegate<TSource, TDestination> BuildFromFlow<TSource, TDestination>(
        Flow flow)
    {
        var sourceParam = Expression.Parameter(typeof(TSource), "source");
        var destVar = Expression.Variable(typeof(TDestination), "dest");

        var bodyExpressions = new List<Expression>
        {
            Expression.Assign(destVar, BuildInstanceCreation<TDestination>())
        };

        foreach (var prop in flow.Properties)
        {
            if (prop.IsIgnored) continue;
            var destProp = destVar.Type.GetProperty(
                prop.DestinationProperty,
                BindingFlags.Public | BindingFlags.Instance);
            if (destProp == null || !destProp.CanWrite) continue;

            bodyExpressions.Add(BuildPropertyMapping(destVar, prop, destProp, sourceParam));
        }

        foreach (var nested in flow.NestedFlows)
        {
            var destProp = destVar.Type.GetProperty(
                nested.ParentProperty,
                BindingFlags.Public | BindingFlags.Instance);
            if (destProp == null || !destProp.CanWrite) continue;

            bodyExpressions.Add(BuildNestedMapping(destVar, nested, destProp, sourceParam));
        }

        bodyExpressions.Add(destVar);

        var body = Expression.Block([destVar], bodyExpressions);
        var lambda = Expression.Lambda<MappingDelegate<TSource, TDestination>>(body, sourceParam);
        return lambda.Compile();
    }

    public MappingDelegate<TSource, TDestination> BuildFromFlow<TSource, TDestination>(
        IMappingArtifact artifact)
    {
        if (artifact.MappingDelegate != null)
            return (MappingDelegate<TSource, TDestination>)artifact.MappingDelegate;

        var sourceParam = Expression.Parameter(typeof(TSource), "source");
        var destVar = Expression.Variable(typeof(TDestination), "dest");

        var bodyExpressions = new List<Expression>
        {
            Expression.Assign(destVar, BuildInstanceCreation<TDestination>())
        };

        var sourceProps = typeof(TSource).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var destProps = typeof(TDestination).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite);

        foreach (var destProp in destProps)
        {
            var sourceProp = sourceProps.FirstOrDefault(p =>
                p.Name == destProp.Name && p.PropertyType == destProp.PropertyType);
            if (sourceProp == null) continue;

            var sourceValue = (Expression)Expression.Property(sourceParam, sourceProp);

            if (sourceProp.PropertyType != destProp.PropertyType)
                sourceValue = Expression.Convert(sourceValue, destProp.PropertyType);

            bodyExpressions.Add(
                Expression.Assign(
                    Expression.Property(destVar, destProp),
                    sourceValue));
        }

        bodyExpressions.Add(destVar);

        var body = Expression.Block([destVar], bodyExpressions);
        var lambda = Expression.Lambda<MappingDelegate<TSource, TDestination>>(body, sourceParam);
        return lambda.Compile();
    }

    private static Expression BuildInstanceCreation<TDestination>()
    {
        var targetType = typeof(TDestination);
        var ctor = targetType.GetConstructor(Type.EmptyTypes);
        if (ctor != null)
            return Expression.New(ctor);

        var firstCtor = targetType.GetConstructors()
            .OrderByDescending(c => c.GetParameters().Length)
            .FirstOrDefault();

        if (firstCtor != null)
        {
            var args = firstCtor.GetParameters()
                .Select(p => Expression.Default(p.ParameterType))
                .ToArray();
            return Expression.New(firstCtor, args);
        }

        throw new InvalidOperationException(
            $"Type {targetType} has no usable constructor.");
    }

    private static Expression BuildPropertyMapping(
        Expression destVar, PropertyFlow prop, PropertyInfo destProp, Expression sourceParam)
    {
        if (prop.IsPathMapping)
            return BuildPathMapping(destVar, prop, destProp, sourceParam);

        Expression sourceValue;

        if (prop.MapFromExpression != null)
        {
            sourceValue = ResolveExpression(sourceParam, prop.MapFromExpression)
                ?? throw new InvalidOperationException(
                    $"Cannot resolve MapFrom expression '{prop.MapFromExpression}'.");
        }
        else
        {
            var sourceProp = sourceParam.Type.GetProperty(
                prop.SourceProperty,
                BindingFlags.Public | BindingFlags.Instance)
                ?? throw new InvalidOperationException(
                    $"Source property '{prop.SourceProperty}' not found on '{sourceParam.Type.Name}'.");

            sourceValue = Expression.Property(sourceParam, sourceProp);
        }

        if (sourceValue.Type != destProp.PropertyType)
            sourceValue = Expression.Convert(sourceValue, destProp.PropertyType);

        return Expression.Assign(
            Expression.Property(destVar, destProp),
            sourceValue);
    }

    private static Expression BuildPathMapping(
        Expression destVar, PropertyFlow prop, PropertyInfo topDestProp, Expression sourceParam)
    {
        if (prop.PathSegments.Count == 0)
            return Expression.Assign(Expression.Property(destVar, topDestProp),
                Expression.Default(topDestProp.PropertyType));

        Expression sourceValue;
        if (prop.MapFromExpression != null)
        {
            sourceValue = ResolveExpression(sourceParam, prop.MapFromExpression)
                ?? throw new InvalidOperationException(
                    $"Cannot resolve MapFrom expression '{prop.MapFromExpression}'.");
        }
        else
        {
            var sourceProp = sourceParam.Type.GetProperty(
                prop.SourceProperty,
                BindingFlags.Public | BindingFlags.Instance);
            if (sourceProp == null)
                return Expression.Assign(Expression.Property(destVar, topDestProp),
                    Expression.Default(topDestProp.PropertyType));
            sourceValue = Expression.Property(sourceParam, sourceProp);
        }

        if (prop.PathSegments.Count == 1)
        {
            if (sourceValue.Type != topDestProp.PropertyType)
                sourceValue = Expression.Convert(sourceValue, topDestProp.PropertyType);
            return Expression.Assign(
                Expression.Property(destVar, topDestProp),
                sourceValue);
        }

        var nestedProp = destVar.Type.GetProperty(
            prop.PathSegments[0],
            BindingFlags.Public | BindingFlags.Instance);
        if (nestedProp == null)
            return Expression.Assign(Expression.Property(destVar, topDestProp),
                Expression.Default(topDestProp.PropertyType));

        var nestedGetter = Expression.Property(destVar, nestedProp);
        var nestedVar = Expression.Variable(nestedProp.PropertyType, "nested");
        var blockExprs = new List<Expression>
        {
            Expression.Assign(nestedVar, nestedGetter)
        };

        Expression currentDest = nestedVar;
        for (var i = 1; i < prop.PathSegments.Count - 1; i++)
        {
            var segProp = currentDest.Type.GetProperty(
                prop.PathSegments[i],
                BindingFlags.Public | BindingFlags.Instance);
            if (segProp == null) break;

            blockExprs.Add(
                Expression.IfThen(
                    Expression.Equal(
                        Expression.Property(currentDest, segProp),
                        Expression.Constant(null, segProp.PropertyType)),
                    Expression.Assign(
                        Expression.Property(currentDest, segProp),
                        Expression.New(segProp.PropertyType))));

            currentDest = Expression.Property(currentDest, segProp);
        }

        var targetProp = currentDest.Type.GetProperty(
            prop.PathSegments[^1],
            BindingFlags.Public | BindingFlags.Instance);
        if (targetProp != null)
        {
            var finalValue = sourceValue;
            if (finalValue.Type != targetProp.PropertyType)
                finalValue = Expression.Convert(finalValue, targetProp.PropertyType);
            blockExprs.Add(
                Expression.Assign(
                    Expression.Property(currentDest, targetProp),
                    finalValue));
        }

        blockExprs.Add(nestedVar);
        return Expression.Block([nestedVar], blockExprs);
    }

    private static Expression BuildNestedMapping(
        Expression destVar, NestedFlow nested, PropertyInfo destProp, Expression sourceParam)
    {
        var sourcePropType = nested.ChildFlow.Signature.SourceType;
        var destPropType = nested.ChildFlow.Signature.DestinationType;

        var sourceProp = sourceParam.Type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(p => p.PropertyType == sourcePropType);

        if (sourceProp == null)
        {
            return Expression.Assign(
                Expression.Property(destVar, destProp),
                Expression.Default(destProp.PropertyType));
        }

        var sourceValue = Expression.Property(sourceParam, sourceProp);

        var nestedDelegate = BuildFromFlowInternal(sourcePropType, destPropType, nested.ChildFlow);

        var nestedCall = Expression.Invoke(
            Expression.Constant(nestedDelegate),
            sourceValue);

        var finalValue = destProp.PropertyType != destPropType
            ? Expression.Convert(nestedCall, destProp.PropertyType)
            : (Expression)nestedCall;

        return Expression.Assign(
            Expression.Property(destVar, destProp),
            finalValue);
    }

    private static Delegate BuildFromFlowInternal(Type sourceType, Type destType, Flow flow)
    {
        var method = typeof(MappingDelegateBuilder)
            .GetMethod("BuildFromFlow", [typeof(Flow)])!
            .MakeGenericMethod(sourceType, destType);

        return (Delegate)method.Invoke(new MappingDelegateBuilder(), [flow])!;
    }

    private static Expression? ResolveExpression(Expression sourceParam, string expression)
    {
        var parts = expression.Split('.');
        Expression current = sourceParam;
        foreach (var part in parts)
        {
            var prop = current.Type.GetProperty(part.Trim(),
                BindingFlags.Public | BindingFlags.Instance);
            if (prop == null) return null;
            current = Expression.Property(current, prop);
        }
        return current;
    }
}
