namespace FlowMapper.Abstractions;

public interface IFlowMapper
{
    TDestination Map<TSource, TDestination>(TSource source);
    IMapper<TSource, TDestination> GetMapper<TSource, TDestination>();

    Task<IEnumerable<T>> QueryAsync<T>(string sql, object? parameters = null, ExecutionOptions? options = null, CancellationToken ct = default);
    Task<T> QuerySingleAsync<T>(string sql, object? parameters = null, ExecutionOptions? options = null, CancellationToken ct = default);
    Task<T?> QuerySingleOrDefaultAsync<T>(string sql, object? parameters = null, ExecutionOptions? options = null, CancellationToken ct = default);
    Task<T> QueryScalarAsync<T>(string sql, object? parameters = null, ExecutionOptions? options = null, CancellationToken ct = default);
    IAsyncEnumerable<T> StreamAsync<T>(string sql, object? parameters = null, ExecutionOptions? options = null, CancellationToken ct = default);

    Task<int> CommandAsync(string sql, object? parameters = null, ExecutionOptions? options = null, CancellationToken ct = default);
    Task<T> CommandScalarAsync<T>(string sql, object? parameters = null, ExecutionOptions? options = null, CancellationToken ct = default);

    T FromJson<T>(string json);
    List<T> FromJsonList<T>(string json);
    T FromXml<T>(string xml);
    List<T> FromText<T>(string[] lines, TextDelimiter delimiter, bool hasHeader = true);

    IExecutionScope CreateScope(bool transactional = false);
}
