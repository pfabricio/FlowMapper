using System.Data;
using Xunit;
using FlowMapper.Abstractions;
using FlowMapper.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace FlowMapper.IntegrationTests;

public class StubDialect : IDialect
{
    public string ApplyPagination(string sql, int offset, int limit) => sql;
    public string GetIdentityQuery() => "SELECT SCOPE_IDENTITY()";
    public string NormalizeParameter(string name) => $"@{name}";
    public string BuildFreeTextCondition(IReadOnlyList<string> columns, string parameterName) => $"{string.Join(", ", columns)} @@ {parameterName}";
    public string BuildContainsCondition(IReadOnlyList<string> columns, string parameterName) => $"CONTAINS({string.Join(", ", columns)}, {parameterName})";
    public string BuildRankOrderBy(IReadOnlyList<string> columns, string parameterName) => "RANK DESC";
    public string? VerifyFtsIndexSql(string table, string column) => null;
    public string? FtsIndexErrorMessage => null;
    public bool FtsRequiresIndex => false;
    public bool FtsSupportsLanguage => false;
}

public class StubDatabaseProvider : IDatabaseProvider
{
    public string Name => "Stub";
    public IDialect Dialect => new StubDialect();
    public Version Version => new(1, 0);
    public IDbConnection CreateConnection() => throw new NotSupportedException();
    public IDbCommand CreateCommand(string sql, IDbConnection connection, IDbTransaction? transaction = null)
        => throw new NotSupportedException();
    public IDataParameter CreateParameter(string name, object? value)
        => throw new NotSupportedException();
}

public class DependencyInjectionTests
{
    [Fact]
    public void AddFlowMapper_Registers_IFlowMapper()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IDatabaseProvider>(new StubDatabaseProvider());
        services.AddFlowMapper();
        var provider = services.BuildServiceProvider();

        var mapper = provider.GetRequiredService<IFlowMapper>();
        Assert.NotNull(mapper);
    }

    [Fact]
    public void AddFlowMapper_Configures_MappingOptions()
    {
        var services = new ServiceCollection();
        services.AddFlowMapper(cfg => cfg.ConfigureMapping(m => m.DefaultProfile = "Api"));
        var provider = services.BuildServiceProvider();

        var options = provider.GetRequiredService<FlowMapperOptions>();
        Assert.Equal("Api", options.Mapping.DefaultProfile);
    }

    [Fact]
    public void AddFlowMapper_DefaultOptions()
    {
        var services = new ServiceCollection();
        services.AddFlowMapper();
        var provider = services.BuildServiceProvider();

        var options = provider.GetRequiredService<FlowMapperOptions>();
        Assert.Equal("Default", options.Mapping.DefaultProfile);
        Assert.True(options.Mapping.EnableFlatten);
        Assert.Equal(StrictnessLevel.None, options.Mapping.Strictness);
    }

    [Fact]
    public void FlowMapperService_Throws_When_Mapper_Not_Found()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IDatabaseProvider>(new StubDatabaseProvider());
        services.AddFlowMapper();
        var provider = services.BuildServiceProvider();

        var mapper = provider.GetRequiredService<IFlowMapper>();
        var ex = Assert.Throws<InvalidOperationException>(() =>
            mapper.Map<UnmappedSource, UnmappedDest>(new UnmappedSource()));

        Assert.Contains("No mapper registered", ex.Message);
    }

    public class UnmappedSource { public int Id { get; set; } }
    public class UnmappedDest { public int Id { get; set; } }

    [Fact]
    public void FlowMapperService_Resolves_Registered_Mapper()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IDatabaseProvider>(new StubDatabaseProvider());
        services.AddFlowMapper();
        services.AddTransient<IMapper<Source, Dest>, ManualMapper>();
        var provider = services.BuildServiceProvider();

        var mapper = provider.GetRequiredService<IFlowMapper>();
        var result = mapper.Map<Source, Dest>(new Source { Id = 5 });

        Assert.NotNull(result);
        Assert.Equal(5, result.Id);
    }

    public class Source { public int Id { get; set; } }
    public class Dest { public int Id { get; set; } }

    public class ManualMapper : IMapper<Source, Dest>
    {
        public Dest Map(Source source) => new() { Id = source.Id };
    }
}