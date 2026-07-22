using FlowMapper.DependencyInjection;
using FlowMapper.Providers.SqlServer;
using Testcontainers.MsSql;
using Xunit;

namespace FlowMapper.IntegrationTests;

public class SqlServerIntegrationTests : DatabaseTestBase
{
    private MsSqlContainer? _container;

    protected override void ConfigureProvider(FlowMapperBuilder builder)
    {
        builder.AddProvider<SqlServerProvider>(_container?.GetConnectionString() ?? "");
    }

    protected override async Task StartContainer()
    {
        _container = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .WithPassword("FlowMapper@123")
            .Build();

        await _container.StartAsync();
    }

    protected override async Task SeedDatabase()
    {
        await Rapid.ExecuteAsync(@"
            CREATE TABLE TestEntities (
                Id INT IDENTITY(1,1) PRIMARY KEY,
                FirstName NVARCHAR(100),
                LastName NVARCHAR(100),
                Email NVARCHAR(200)
            )");

        await Rapid.ExecuteAsync(@"
            INSERT INTO TestEntities (FirstName, LastName, Email) VALUES
            (N'João', N'Silva', 'joao@email.com'),
            (N'Maria', N'Santos', 'maria@email.com')");
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
            "SELECT TOP 1 Id, FirstName, LastName, Email FROM TestEntities");

        Assert.NotNull(result);
        Assert.Equal("João", result.FirstName);
    }

    [Fact]
    public async Task ExecuteAsync_InsertsAndReturnsAffected()
    {
        var affected = await Rapid.ExecuteAsync(
            "INSERT INTO TestEntities (FirstName, LastName, Email) VALUES (N'Pedro', N'Oliveira', 'pedro@email.com')");

        Assert.Equal(1, affected);
    }

    [Fact]
    public async Task QueryScalarAsync_ReturnsScalarValue()
    {
        var count = await Rapid.QueryScalarAsync<int>("SELECT COUNT(*) FROM TestEntities");

        Assert.True(count > 0);
    }
}
