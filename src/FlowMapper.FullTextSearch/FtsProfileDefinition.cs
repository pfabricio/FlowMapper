using System.Linq.Expressions;

namespace FlowMapper.FullTextSearch;

public abstract class FtsProfileDefinition
{
    private readonly FullTextIndexRegistry _registry = new();

    public IFullTextIndexRegistry Registry => _registry;

    protected FtsEntityBuilder<T> Entity<T>()
    {
        return new FtsEntityBuilder<T>(_registry);
    }
}

public sealed class FtsEntityBuilder<T>
{
    private readonly FullTextIndexRegistry _registry;

    public FtsEntityBuilder(FullTextIndexRegistry registry)
    {
        _registry = registry;
    }

    public FtsEntityBuilder<T> HasFullTextIndex(Expression<Func<T, object?>> propertyExpression)
    {
        var columnName = ExtractPropertyName(propertyExpression);
        _registry.Register<T>(columnName);
        return this;
    }

    private static string ExtractPropertyName(Expression<Func<T, object?>> expression)
    {
        if (expression.Body is MemberExpression member)
            return member.Member.Name;

        if (expression.Body is UnaryExpression unary && unary.Operand is MemberExpression memberOperand)
            return memberOperand.Member.Name;

        throw new ArgumentException("Expression must be a property access expression.");
    }
}
