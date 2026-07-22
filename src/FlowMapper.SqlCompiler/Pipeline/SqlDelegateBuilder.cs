using System.Linq.Expressions;
using System.Reflection;
using FlowMapper.Execution.Artifacts;

namespace FlowMapper.SqlCompiler.Pipeline;

public class SqlDelegateBuilder
{
    public virtual SqlDelegate BuildDelegate()
    {
        return (sql, parameters) =>
        {
            var bindings = new List<IParameterBinding>();

            if (parameters != null)
            {
                if (parameters is IDictionary<string, object> dict)
                {
                    foreach (var kvp in dict)
                        bindings.Add(new ParameterBinding(kvp.Key, kvp.Value?.GetType() ?? typeof(object), null));
                }
                else
                {
                    foreach (var prop in parameters.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
                        bindings.Add(new ParameterBinding(prop.Name, prop.PropertyType, null));
                }
            }

            return new CompiledSql(sql, bindings);
        };
    }

    public Delegate BuildParameterBinder(Type parametersType)
    {
        var objParam = Expression.Parameter(typeof(object), "parameters");
        var paramExpr = Expression.Variable(parametersType, "typed");
        var listExpr = Expression.Variable(typeof(List<IParameterBinding>), "bindings");
        var listType = typeof(List<IParameterBinding>);

        var bodyExprs = new List<Expression>
        {
            Expression.Assign(paramExpr, Expression.Convert(objParam, parametersType)),
            Expression.Assign(listExpr, Expression.New(listType))
        };

        foreach (var prop in parametersType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var getValue = Expression.Property(paramExpr, prop);
            var binding = Expression.New(
                typeof(ParameterBinding).GetConstructor([typeof(string), typeof(Type), typeof(int?)])!,
                Expression.Constant(prop.Name),
                Expression.Constant(prop.PropertyType),
                Expression.Constant(null, typeof(int?)));

            bodyExprs.Add(
                Expression.Call(listExpr, listType.GetMethod("Add")!, binding));
        }

        bodyExprs.Add(listExpr);

        var body = Expression.Block([paramExpr, listExpr], bodyExprs);
        var lambda = Expression.Lambda<Func<object, List<IParameterBinding>>>(body, objParam);
        return lambda.Compile();
    }
}
