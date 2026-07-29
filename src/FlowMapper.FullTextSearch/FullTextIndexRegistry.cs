using System.Linq.Expressions;

namespace FlowMapper.FullTextSearch;

public sealed class FullTextIndexRegistry : IFullTextIndexRegistry
{
    private sealed record IndexKey(string Table, string Column);

    private readonly Dictionary<IndexKey, FtsIndexState> _states = new();
    private readonly Dictionary<Type, List<string>> _columnsByType = new();

    public void Register<T>(string column)
    {
        var table = typeof(T).Name;
        var key = new IndexKey(table, column);

        if (!_states.ContainsKey(key))
            _states[key] = FtsIndexState.Configured;

        if (!_columnsByType.TryGetValue(typeof(T), out var columns))
        {
            columns = new List<string>();
            _columnsByType[typeof(T)] = columns;
        }

        if (!columns.Contains(column))
            columns.Add(column);
    }

    public bool IsConfigured<T>(Expression<Func<T, object?>> propertyExpression)
    {
        var columnName = ExtractPropertyName(propertyExpression);
        var table = typeof(T).Name;
        return _states.ContainsKey(new IndexKey(table, columnName));
    }

    public IReadOnlyList<string> GetConfiguredColumns<T>()
    {
        if (_columnsByType.TryGetValue(typeof(T), out var columns))
            return columns.AsReadOnly();
        return Array.Empty<string>();
    }

    public IReadOnlyList<(string Table, string Column)> GetAllConfigured()
    {
        return _states.Keys
            .Select(k => (k.Table, k.Column))
            .ToList();
    }

    public FtsIndexState GetState(string table, string column)
    {
        return _states.TryGetValue(new IndexKey(table, column), out var state) ? state : FtsIndexState.Unknown;
    }

    public void SetState(string table, string column, FtsIndexState state)
    {
        var key = new IndexKey(table, column);
        if (_states.ContainsKey(key))
            _states[key] = state;
    }

    private static string ExtractPropertyName<T>(Expression<Func<T, object?>> expression)
    {
        if (expression.Body is MemberExpression member)
            return member.Member.Name;

        if (expression.Body is UnaryExpression unary && unary.Operand is MemberExpression memberOperand)
            return memberOperand.Member.Name;

        throw new ArgumentException("Expression must be a property access expression.");
    }
}
