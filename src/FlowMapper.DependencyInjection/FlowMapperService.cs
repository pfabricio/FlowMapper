using FlowMapper.Abstractions;
using FlowMapper.Deserialization;
using FlowMapper.FullTextSearch;
using Microsoft.Extensions.DependencyInjection;

namespace FlowMapper.DependencyInjection;

public class FlowMapperService : IFlowMapper
{
    private readonly IServiceProvider _sp;
    private readonly IRapidMapper _rapid;
    private readonly IDeserializer _deserializer;

    public FlowMapperService(IServiceProvider sp, IRapidMapper rapid, IDeserializer deserializer)
    {
        _sp = sp;
        _rapid = rapid;
        _deserializer = deserializer;
    }

    public TDestination Map<TSource, TDestination>(TSource source)
    {
        var mapper = GetMapper<TSource, TDestination>();
        return mapper.Map(source);
    }

    public IMapper<TSource, TDestination> GetMapper<TSource, TDestination>()
    {
        var mapper = _sp.GetService<IMapper<TSource, TDestination>>();
        if (mapper != null) return mapper;

        var mappers = _sp.GetServices<IMapper<TSource, TDestination>>();
        mapper = mappers.FirstOrDefault();
        if (mapper != null) return mapper;

        throw new InvalidOperationException(
            $"No mapper registered for {typeof(TSource).Name} → {typeof(TDestination).Name}. " +
            "Ensure the source generator ran and the mapper class was discovered.");
    }

    public Task<IEnumerable<T>> QueryAsync<T>(string sql, object? parameters = null, ExecutionOptions? options = null, CancellationToken ct = default)
        => _rapid.QueryAsync<T>(sql, parameters, options, ct);

    public Task<T> QuerySingleAsync<T>(string sql, object? parameters = null, ExecutionOptions? options = null, CancellationToken ct = default)
        => _rapid.QuerySingleAsync<T>(sql, parameters, options, ct);

    public Task<T?> QuerySingleOrDefaultAsync<T>(string sql, object? parameters = null, ExecutionOptions? options = null, CancellationToken ct = default)
        => _rapid.QuerySingleOrDefaultAsync<T>(sql, parameters, options, ct);

    public Task<T> QueryScalarAsync<T>(string sql, object? parameters = null, ExecutionOptions? options = null, CancellationToken ct = default)
        => _rapid.QueryScalarAsync<T>(sql, parameters, options, ct);

    public IAsyncEnumerable<T> StreamAsync<T>(string sql, object? parameters = null, ExecutionOptions? options = null, CancellationToken ct = default)
        => _rapid.StreamAsync<T>(sql, parameters, options, ct);

    public Task<int> CommandAsync(string sql, object? parameters = null, ExecutionOptions? options = null, CancellationToken ct = default)
        => _rapid.ExecuteAsync(sql, parameters, options, ct);

    public Task<T> CommandScalarAsync<T>(string sql, object? parameters = null, ExecutionOptions? options = null, CancellationToken ct = default)
        => _rapid.ExecuteScalarAsync<T>(sql, parameters, options, ct);

    public T FromJson<T>(string json)
        => _deserializer.FromJson<T>(json);

    public List<T> FromJsonList<T>(string json)
        => _deserializer.FromJsonList<T>(json);

    public T FromXml<T>(string xml)
        => _deserializer.FromXml<T>(xml);

    public List<T> FromText<T>(string[] lines, TextDelimiter delimiter, bool hasHeader = true)
        => _deserializer.FromText<T>(lines, delimiter, hasHeader);

    public async Task<IEnumerable<T>> SearchFtsAsync<T>(string sql, string searchTerm, string[] columns, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(sql))
            throw new ArgumentException("SQL cannot be null or empty.", nameof(sql));
        if (string.IsNullOrWhiteSpace(searchTerm))
            throw new ArgumentException("Search term cannot be null or empty.", nameof(searchTerm));
        if (columns is null || columns.Length == 0)
            throw new ArgumentException("Columns array cannot be null or empty.", nameof(columns));

        var provider = _sp.GetRequiredService<IDatabaseProvider>();
        var ftsCondition = provider.Dialect.BuildFreeTextCondition(columns, "@term");
        var modifiedSql = FtsSqlInjector.InjectFtsCondition(sql, ftsCondition);

        return await _rapid.QueryAsync<T>(modifiedSql, new { term = searchTerm }, null, ct).ConfigureAwait(false);
    }

    public IExecutionScope CreateScope(bool transactional = false)
        => _rapid.CreateScope(transactional);
}
