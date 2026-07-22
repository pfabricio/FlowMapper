using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using FlowMapper.Execution.Artifacts;

namespace FlowMapper.Materializer.Pipeline;

public class MaterializationDelegateBuilder
{
    private static readonly PropertyInfo _intItemProperty = typeof(IDataRecord)
        .GetProperties()
        .First(p =>
        {
            var idx = p.GetIndexParameters();
            return idx.Length == 1 && idx[0].ParameterType == typeof(int);
        });

    private static readonly MethodInfo _getOrdinalMethod = typeof(IDataRecord)
        .GetMethod("GetOrdinal", [typeof(string)])!;

    private static readonly MethodInfo _isDBNullIntMethod = typeof(IDataRecord)
        .GetMethod("IsDBNull", [typeof(int)])!;

    private static readonly MethodInfo _changeTypeMethod = typeof(Convert)
        .GetMethod("ChangeType", [typeof(object), typeof(Type)])!;

    private static readonly MethodInfo _enumToObject = typeof(Enum)
        .GetMethod("ToObject", [typeof(Type), typeof(object)])!;

    private static readonly MethodInfo _guidParse = typeof(Guid)
        .GetMethod("Parse", [typeof(string)])!;

    private static readonly MethodInfo _toStringMethod = typeof(object)
        .GetMethod("ToString")!;

    private readonly string _separator;

    public MaterializationDelegateBuilder(string? separator = "_")
    {
        _separator = separator ?? "_";
    }

    public virtual MaterializationDelegate<T> BuildDelegate<T>(IMaterializationArtifact artifact)
    {
        var readerParam = Expression.Parameter(typeof(IDataReader), "reader");
        var targetType = typeof(T);

        var instanceExpr = BuildInstanceCreation(targetType, artifact, readerParam);
        var instanceVar = Expression.Variable(targetType, "instance");

        var bodyExpressions = new List<Expression>
        {
            Expression.Assign(instanceVar, instanceExpr)
        };

        var (flatBindings, nestedGroups) = GroupBindings(artifact.ColumnBindings, targetType);

        foreach (var binding in flatBindings)
        {
            var prop = targetType.GetProperty(binding.MemberName, BindingFlags.Public | BindingFlags.Instance);
            if (prop == null || !prop.CanWrite) continue;

            bodyExpressions.Add(BuildPropertyBinding(instanceVar, binding, prop, readerParam));
        }

        foreach (var group in nestedGroups)
        {
            var prop = targetType.GetProperty(group.Key, BindingFlags.Public | BindingFlags.Instance);
            if (prop == null || !prop.CanWrite) continue;

            bodyExpressions.Add(BuildNestedAssignment(instanceVar, group.Key, prop, group.Value, readerParam));
        }

        bodyExpressions.Add(instanceVar);

        var body = Expression.Block([instanceVar], bodyExpressions);
        var lambda = Expression.Lambda<MaterializationDelegate<T>>(body, readerParam);
        return lambda.Compile();
    }

    private (List<IColumnBinding> flat, Dictionary<string, List<IColumnBinding>> nested)
        GroupBindings(IReadOnlyCollection<IColumnBinding> bindings, Type targetType)
    {
        var flat = new List<IColumnBinding>();
        var nested = new Dictionary<string, List<IColumnBinding>>();

        foreach (var prop in targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (IsComplexType(prop.PropertyType))
            {
                var prefix = prop.Name + _separator;
                List<IColumnBinding> sub = bindings
                    .Where(b => b.MemberName.StartsWith(prefix))
                    .Select(b => new ColumnBinding(
                        b.ColumnName,
                        b.MemberName.Substring(prefix.Length),
                        b.MemberType,
                        b.Converter,
                        b.IsNullable))
                    .Cast<IColumnBinding>()
                    .ToList();
                if (sub.Any())
                    nested[prop.Name] = sub;
            }
            else
            {
                var match = bindings.FirstOrDefault(b => b.MemberName == prop.Name);
                if (match != null)
                    flat.Add(match);
            }
        }

        return (flat, nested);
    }

    private static bool IsComplexType(Type type)
    {
        return type.IsClass
            && type != typeof(string)
            && !type.IsValueType
            && !typeof(System.Collections.IEnumerable).IsAssignableFrom(type);
    }

    private Expression BuildNestedAssignment(
        Expression instanceVar, string propertyName, PropertyInfo prop,
        List<IColumnBinding> subBindings, Expression readerParam)
    {
        var nestedType = prop.PropertyType;
        var subArtifact = new MaterializationArtifact(
            Name: $"Nested_{propertyName}",
            Version: new Version(2, 0),
            TargetType: nestedType,
            Separator: _separator,
            ConstructorDelegate: null,
            ColumnBindings: subBindings,
            MaterializationDelegate: null
        );

        var buildDelegateMethod = typeof(MaterializationDelegateBuilder)
            .GetMethod(nameof(BuildDelegate), BindingFlags.Public | BindingFlags.Instance)!;
        var genericMethod = buildDelegateMethod.MakeGenericMethod(nestedType);
        var subDelegate = genericMethod.Invoke(this, [subArtifact]);

        var delegateExpr = Expression.Constant(subDelegate);
        var invokeExpr = Expression.Invoke(delegateExpr, readerParam);

        var firstBinding = subBindings[0];
        var columnNameExpr = Expression.Constant(firstBinding.ColumnName);
        var ordinalExpr = Expression.Call(readerParam, _getOrdinalMethod, columnNameExpr);
        var isNullExpr = Expression.Call(readerParam, _isDBNullIntMethod, ordinalExpr);

        var propExpr = Expression.Property(instanceVar, prop);
        var nullExpr = Expression.Constant(null, nestedType);

        return Expression.IfThen(
            Expression.Not(isNullExpr),
            Expression.Assign(propExpr, Expression.Convert(invokeExpr, nestedType))
        );
    }

    private static Expression BuildInstanceCreation(
        Type targetType, IMaterializationArtifact artifact, Expression readerParam)
    {
        if (artifact.ConstructorDelegate != null)
        {
            return Expression.Invoke(
                Expression.Constant(artifact.ConstructorDelegate),
                readerParam);
        }

        var parameterlessCtor = targetType.GetConstructor(Type.EmptyTypes);
        if (parameterlessCtor != null)
            return Expression.New(parameterlessCtor);

        var ctorWithParams = targetType.GetConstructors()
            .OrderByDescending(c => c.GetParameters().Length)
            .FirstOrDefault();

        if (ctorWithParams != null)
        {
            var paramInfos = ctorWithParams.GetParameters();
            var args = new Expression[paramInfos.Length];
            for (var i = 0; i < paramInfos.Length; i++)
            {
                var p = paramInfos[i];
                var defaultAttr = p.GetCustomAttribute<System.ComponentModel.DefaultValueAttribute>();
                args[i] = defaultAttr != null
                    ? Expression.Constant(defaultAttr.Value, p.ParameterType)
                    : Expression.Default(p.ParameterType);
            }
            return Expression.New(ctorWithParams, args);
        }

        throw new InvalidOperationException(
            $"Type {targetType} has no usable constructor for materialization.");
    }

    private static Expression BuildPropertyBinding(
        Expression instanceVar, IColumnBinding binding, PropertyInfo prop, Expression readerParam)
    {
        var columnNameExpr = Expression.Constant(binding.ColumnName);

        var ordinalExpr = Expression.Call(readerParam, _getOrdinalMethod, columnNameExpr);

        var getValueExpr = Expression.MakeIndex(
            readerParam, _intItemProperty, [ordinalExpr]);

        var isNullExpr = Expression.Call(readerParam, _isDBNullIntMethod, ordinalExpr);

        var convertedExpr = BuildValueConverter(getValueExpr, prop.PropertyType, binding.Converter);

        return Expression.IfThen(
            Expression.Not(isNullExpr),
            Expression.Assign(Expression.Property(instanceVar, prop), convertedExpr));
    }

    private static Expression BuildValueConverter(
        Expression rawValue, Type targetType, Delegate? converter)
    {
        if (converter != null)
        {
            return Expression.Convert(
                Expression.Invoke(Expression.Constant(converter), rawValue),
                targetType);
        }

        var underlyingType = Nullable.GetUnderlyingType(targetType);
        var actualTarget = underlyingType ?? targetType;

        if (actualTarget.IsEnum)
        {
            var converted = Expression.Convert(
                Expression.Call(_enumToObject,
                    Expression.Constant(actualTarget),
                    Expression.Convert(rawValue, typeof(object))),
                targetType);
            return converted;
        }

        if (actualTarget == typeof(Guid))
        {
            var converted = Expression.Call(_guidParse,
                Expression.Call(
                    Expression.Convert(rawValue, typeof(object)),
                    _toStringMethod));
            return underlyingType != null
                ? Expression.Convert(converted, targetType)
                : converted;
        }

        if (actualTarget == typeof(string))
        {
            return Expression.Condition(
                Expression.TypeIs(rawValue, typeof(DBNull)),
                Expression.Constant(null, typeof(string)),
                Expression.Call(
                    Expression.Convert(rawValue, typeof(object)),
                    _toStringMethod));
        }

        if (actualTarget.IsPrimitive || actualTarget == typeof(decimal))
        {
            var changeTypeCall = Expression.Convert(
                Expression.Call(_changeTypeMethod,
                    Expression.Convert(rawValue, typeof(object)),
                    Expression.Constant(actualTarget)),
                targetType);
            return changeTypeCall;
        }

        return Expression.Convert(rawValue, targetType);
    }
}
