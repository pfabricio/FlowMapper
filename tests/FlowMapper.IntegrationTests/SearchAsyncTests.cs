using System.Collections;
using System.Data;
using FlowMapper.Abstractions;
using FlowMapper.DependencyInjection;
using FlowMapper.Deserialization;
using FlowMapper.FullTextSearch;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FlowMapper.IntegrationTests;

public class SpyDialect : IDialect
{
    public string LastFtsCondition { get; private set; } = string.Empty;

    public string ApplyPagination(string sql, int offset, int limit) => sql;
    public string GetIdentityQuery() => "SELECT SCOPE_IDENTITY()";
    public string NormalizeParameter(string name) => $"@{name}";

    public string BuildFreeTextCondition(IReadOnlyList<string> columns, string parameterName)
    {
        LastFtsCondition = $"{string.Join(", ", columns)} @@ {parameterName}";
        return LastFtsCondition;
    }

    public string BuildContainsCondition(IReadOnlyList<string> columns, string parameterName)
        => $"CONTAINS({string.Join(", ", columns)}, {parameterName})";
    public string BuildRankOrderBy(IReadOnlyList<string> columns, string parameterName) => "RANK DESC";
    public string? VerifyFtsIndexSql(string table, string column) => null;
    public string? FtsIndexErrorMessage => null;
    public bool FtsRequiresIndex => false;
    public bool FtsSupportsLanguage => false;
}

public class SpyDatabaseProvider : IDatabaseProvider
{
    public SpyDialect DialectInstance { get; } = new();
    public IDialect Dialect => DialectInstance;
    public string Name => "Spy";
    public Version Version => new(1, 0);
    public IDbConnection CreateConnection() => throw new NotSupportedException();
    public IDbCommand CreateCommand(string sql, IDbConnection connection, IDbTransaction? transaction = null)
        => throw new NotSupportedException();
    public IDataParameter CreateParameter(string name, object? value)
        => throw new NotSupportedException();
}

public class SpyRapidMapper : IRapidMapper
{
    public string? LastSql { get; private set; }
    public object? LastParameters { get; private set; }
    public string? LastSearchTerm => LastParameters?.GetType().GetProperty("term")?.GetValue(LastParameters)?.ToString();

    public Task<IEnumerable<T>> QueryAsync<T>(string sql, object? parameters = null, ExecutionOptions? options = null, CancellationToken ct = default)
    {
        LastSql = sql;
        LastParameters = parameters;
        return Task.FromResult(Enumerable.Empty<T>());
    }

    public Task<T> QuerySingleAsync<T>(string sql, object? parameters = null, ExecutionOptions? options = null, CancellationToken ct = default)
        => throw new NotSupportedException();
    public Task<T?> QuerySingleOrDefaultAsync<T>(string sql, object? parameters = null, ExecutionOptions? options = null, CancellationToken ct = default)
        => throw new NotSupportedException();
    public Task<T> QueryScalarAsync<T>(string sql, object? parameters = null, ExecutionOptions? options = null, CancellationToken ct = default)
        => throw new NotSupportedException();
    public Task<int> ExecuteAsync(string sql, object? parameters = null, ExecutionOptions? options = null, CancellationToken ct = default)
        => throw new NotSupportedException();
    public Task<T> ExecuteScalarAsync<T>(string sql, object? parameters = null, ExecutionOptions? options = null, CancellationToken ct = default)
        => throw new NotSupportedException();
    public IAsyncEnumerable<T> StreamAsync<T>(string sql, object? parameters = null, ExecutionOptions? options = null, CancellationToken ct = default)
        => throw new NotSupportedException();
    public IExecutionScope CreateScope(bool transactional = false)
        => throw new NotSupportedException();
}

public class FakeDeserializer : IDeserializer
{
    public T FromJson<T>(string json) => throw new NotSupportedException();
    public List<T> FromJsonList<T>(string json) => throw new NotSupportedException();
    public T FromXml<T>(string xml) => throw new NotSupportedException();
    public List<T> FromText<T>(string[] lines, TextDelimiter delimiter, bool hasHeader = true) => throw new NotSupportedException();
}

public class SearchAsyncTests
{
    [Fact]
    public async Task SearchAsync_WithoutWhere_InjectsFtsBeforeEnd()
    {
        var provider = new SpyDatabaseProvider();
        var rapid = new SpyRapidMapper();
        var services = new ServiceCollection()
            .AddSingleton<IDatabaseProvider>(provider)
            .AddSingleton<IRapidMapper>(rapid)
            .AddSingleton<IDeserializer>(new FakeDeserializer())
            .AddSingleton<IFullTextIndexRegistry>(new FullTextIndexRegistry())
            .BuildServiceProvider();

        var mapper = new FlowMapperService(services, rapid, new FakeDeserializer());
        var result = await mapper.SearchAsync<object>("SELECT * FROM Produtos", "notebook", ["Nome"]);

        Assert.Contains("WHERE", rapid.LastSql);
        Assert.Contains(provider.DialectInstance.LastFtsCondition, rapid.LastSql);
        Assert.Equal("notebook", rapid.LastSearchTerm);
    }

    [Fact]
    public async Task SearchAsync_WithWhere_InjectsFtsAnd()
    {
        var provider = new SpyDatabaseProvider();
        var rapid = new SpyRapidMapper();
        var services = new ServiceCollection()
            .AddSingleton<IDatabaseProvider>(provider)
            .AddSingleton<IRapidMapper>(rapid)
            .AddSingleton<IDeserializer>(new FakeDeserializer())
            .AddSingleton<IFullTextIndexRegistry>(new FullTextIndexRegistry())
            .BuildServiceProvider();

        var mapper = new FlowMapperService(services, rapid, new FakeDeserializer());
        var result = await mapper.SearchAsync<object>("SELECT * FROM Produtos WHERE Ativo = 1", "notebook", ["Nome"]);

        Assert.Contains("AND", rapid.LastSql);
        Assert.Contains(provider.DialectInstance.LastFtsCondition, rapid.LastSql);
        Assert.Equal("notebook", rapid.LastSearchTerm);
    }

    [Fact]
    public async Task SearchAsync_WithWhereAndOrderBy_InjectsFtsBeforeOrderBy()
    {
        var provider = new SpyDatabaseProvider();
        var rapid = new SpyRapidMapper();
        var services = new ServiceCollection()
            .AddSingleton<IDatabaseProvider>(provider)
            .AddSingleton<IRapidMapper>(rapid)
            .AddSingleton<IDeserializer>(new FakeDeserializer())
            .AddSingleton<IFullTextIndexRegistry>(new FullTextIndexRegistry())
            .BuildServiceProvider();

        var mapper = new FlowMapperService(services, rapid, new FakeDeserializer());
        var result = await mapper.SearchAsync<object>("SELECT * FROM Produtos WHERE Ativo = 1 ORDER BY Nome", "notebook", ["Nome"]);

        Assert.Contains("AND", rapid.LastSql);
        Assert.True(rapid.LastSql!.IndexOf("AND") < rapid.LastSql.IndexOf("ORDER BY"));
        Assert.Equal("notebook", rapid.LastSearchTerm);
    }

    [Fact]
    public async Task SearchAsync_PassesSearchTermAsParameter()
    {
        var provider = new SpyDatabaseProvider();
        var rapid = new SpyRapidMapper();
        var services = new ServiceCollection()
            .AddSingleton<IDatabaseProvider>(provider)
            .AddSingleton<IRapidMapper>(rapid)
            .AddSingleton<IDeserializer>(new FakeDeserializer())
            .AddSingleton<IFullTextIndexRegistry>(new FullTextIndexRegistry())
            .BuildServiceProvider();

        var mapper = new FlowMapperService(services, rapid, new FakeDeserializer());
        var result = await mapper.SearchAsync<object>("SELECT * FROM Produtos", "laptop dell", ["Nome"]);

        Assert.Equal("laptop dell", rapid.LastSearchTerm);
        Assert.Contains("@term", rapid.LastSql);
    }

    [Fact]
    public async Task SearchAsync_CallsBuildFreeTextCondition()
    {
        var provider = new SpyDatabaseProvider();
        var rapid = new SpyRapidMapper();
        var services = new ServiceCollection()
            .AddSingleton<IDatabaseProvider>(provider)
            .AddSingleton<IRapidMapper>(rapid)
            .AddSingleton<IDeserializer>(new FakeDeserializer())
            .AddSingleton<IFullTextIndexRegistry>(new FullTextIndexRegistry())
            .BuildServiceProvider();

        var mapper = new FlowMapperService(services, rapid, new FakeDeserializer());
        var result = await mapper.SearchAsync<object>("SELECT * FROM Produtos", "test", ["Nome", "Descricao"]);

        Assert.Equal("Nome, Descricao @@ @term", provider.DialectInstance.LastFtsCondition);
    }

    [Fact]
    public async Task SearchAsync_ThrowsOnEmptySql()
    {
        var provider = new SpyDatabaseProvider();
        var rapid = new SpyRapidMapper();
        var services = new ServiceCollection()
            .AddSingleton<IDatabaseProvider>(provider)
            .BuildServiceProvider();

        var mapper = new FlowMapperService(services, rapid, new FakeDeserializer());
        await Assert.ThrowsAsync<ArgumentException>(() => mapper.SearchAsync<object>("", "test", ["Nome"]));
    }

    [Fact]
    public async Task SearchAsync_ThrowsOnEmptySearchTerm()
    {
        var provider = new SpyDatabaseProvider();
        var rapid = new SpyRapidMapper();
        var services = new ServiceCollection()
            .AddSingleton<IDatabaseProvider>(provider)
            .BuildServiceProvider();

        var mapper = new FlowMapperService(services, rapid, new FakeDeserializer());
        await Assert.ThrowsAsync<ArgumentException>(() => mapper.SearchAsync<object>("SELECT * FROM T", "", ["Nome"]));
    }

    [Fact]
    public async Task SearchAsync_ThrowsOnNullColumns()
    {
        var provider = new SpyDatabaseProvider();
        var rapid = new SpyRapidMapper();
        var services = new ServiceCollection()
            .AddSingleton<IDatabaseProvider>(provider)
            .BuildServiceProvider();

        var mapper = new FlowMapperService(services, rapid, new FakeDeserializer());
        await Assert.ThrowsAsync<ArgumentException>(() => mapper.SearchAsync<object>("SELECT * FROM T", "test", null!));
    }

    [Fact]
    public async Task SearchAsync_ThrowsOnEmptyColumns()
    {
        var provider = new SpyDatabaseProvider();
        var rapid = new SpyRapidMapper();
        var services = new ServiceCollection()
            .AddSingleton<IDatabaseProvider>(provider)
            .BuildServiceProvider();

        var mapper = new FlowMapperService(services, rapid, new FakeDeserializer());
        await Assert.ThrowsAsync<ArgumentException>(() => mapper.SearchAsync<object>("SELECT * FROM T", "test", []));
    }
}
