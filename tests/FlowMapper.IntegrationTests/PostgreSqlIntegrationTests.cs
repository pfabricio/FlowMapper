using FlowMapper.DependencyInjection;
using FlowMapper.Providers.PostgreSql;
using Testcontainers.PostgreSql;
using Xunit;

namespace FlowMapper.IntegrationTests;

public class PostgreSqlIntegrationTests : DatabaseTestBase
{
    private PostgreSqlContainer? _container;

    protected override void ConfigureProvider(FlowMapperBuilder builder)
    {
        builder.AddProvider<PostgreSqlProvider>(_container?.GetConnectionString() ?? "");
    }

    protected override async Task StartContainer()
    {
        _container = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("flowmapper_test")
            .WithUsername("flowmapper")
            .WithPassword("flowmapper123")
            .Build();

        await _container.StartAsync();
    }

    protected override async Task SeedDatabase()
    {
        await Rapid.ExecuteAsync(@"
            CREATE TABLE TestEntities (
                Id SERIAL PRIMARY KEY,
                FirstName VARCHAR(100),
                LastName VARCHAR(100),
                Email VARCHAR(200)
            )");

        await Rapid.ExecuteAsync(@"
            INSERT INTO TestEntities (FirstName, LastName, Email) VALUES
            ('João', 'Silva', 'joao@email.com'),
            ('Maria', 'Santos', 'maria@email.com')");
    }

    public override async Task DisposeAsync()
    {
        if (_container != null)
            await _container.DisposeAsync();
    }

    [Fact]
    public async Task QueryAsync_ReturnsMappedResults()
    {
        var results = await Rapid
            .QueryAsync<TestEntity>("SELECT Id, FirstName, LastName, Email FROM TestEntities")
            .Map<TestEntity, TestDto>();

        Assert.Equal(2, results.Count);
        Assert.Equal("João Silva", results[0].FullName);
        Assert.Equal("Maria Santos", results[1].FullName);
    }

    [Fact]
    public async Task QuerySingleAsync_ReturnsFirstMatch()
    {
        var result = await Rapid.QuerySingleAsync<TestEntity>(
            "SELECT Id, FirstName, LastName, Email FROM TestEntities LIMIT 1");

        Assert.NotNull(result);
        Assert.Equal("João", result.FirstName);
    }

    [Fact]
    public async Task ExecuteAsync_InsertsAndReturnsAffected()
    {
        var affected = await Rapid.ExecuteAsync(
            "INSERT INTO TestEntities (FirstName, LastName, Email) VALUES ('Pedro', 'Oliveira', 'pedro@email.com')");

        Assert.Equal(1, affected);
    }

    [Fact]
    public async Task QueryScalarAsync_ReturnsScalarValue()
    {
        var count = await Rapid.QueryScalarAsync<int>("SELECT COUNT(*) FROM TestEntities");

        Assert.True(count > 0);
    }
}
