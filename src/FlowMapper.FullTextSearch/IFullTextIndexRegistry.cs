using System.Linq.Expressions;

namespace FlowMapper.FullTextSearch;

public interface IFullTextIndexRegistry
{
    bool IsConfigured<T>(Expression<Func<T, object?>> propertyExpression);
    IReadOnlyList<string> GetConfiguredColumns<T>();
    IReadOnlyList<(string Table, string Column)> GetAllConfigured();
    FtsIndexState GetState(string table, string column);
    void SetState(string table, string column, FtsIndexState state);
}
