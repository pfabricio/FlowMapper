using System.Data;
using System.Threading;
using System.Threading.Tasks;
using FlowMapper.Abstractions;
using FlowMapper.Core;
using FlowMapper.DependencyInjection;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace FlowMapper.Tests.DependencyInjection;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddFlowMapper_WithoutConfiguration_RegistersServices()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IDatabaseProvider, MockDatabaseProvider>();

        services.AddFlowMapper();

        var sp = services.BuildServiceProvider();
        var mapper = sp.GetService<IRapidMapper>();
        var flowMapper = sp.GetService<IFlowMapper>();

        mapper.Should().NotBeNull();
        flowMapper.Should().NotBeNull();
    }

    [Fact]
    public void AddFlowMapper_WithConfigureData_CallsConfigure()
    {
        var services = new ServiceCollection();
        var configured = false;

        services.AddFlowMapper(builder =>
        {
            builder.ConfigureData(options =>
            {
                options.DefaultTimeout = 30;
                configured = true;
            });
        });

        configured.Should().BeTrue();

        var sp = services.BuildServiceProvider();
        var options = sp.GetService<global::FlowMapper.Core.FlowMapperOptions>();
        options.Should().NotBeNull();
        options!.Data.DefaultTimeout.Should().Be(30);
    }

    [Fact]
    public void AddFlowMapper_WithConfigureMapping_CallsConfigure()
    {
        var services = new ServiceCollection();
        var configured = false;

        services.AddFlowMapper(builder =>
        {
            builder.ConfigureMapping(options =>
            {
                options.DefaultProfile = "CustomProfile";
                configured = true;
            });
        });

        configured.Should().BeTrue();
    }

    [Fact]
    public void AddFlowMapper_WithRetry_ConfiguresRetryStrategy()
    {
        var services = new ServiceCollection();

        services.AddFlowMapper(builder =>
        {
            builder.UseRetryStrategy(maxRetries: 5, initialDelayMs: 200);
        });

        var sp = services.BuildServiceProvider();
        var options = sp.GetService<global::FlowMapper.Core.FlowMapperOptions>();

        options.Should().NotBeNull();
        options!.Data.Retry.Enabled.Should().BeTrue();
        options.Data.Retry.MaxRetries.Should().Be(5);
        options.Data.Retry.InitialDelayMs.Should().Be(200);
    }

    [Fact]
    public void AddFlowMapper_WithProvider_RegistersProvider()
    {
        var services = new ServiceCollection();

        services.AddFlowMapper(builder =>
        {
            builder.AddProvider<MockDatabaseProvider>();
        });

        var sp = services.BuildServiceProvider();
        var provider = sp.GetService<IDatabaseProvider>();

        provider.Should().NotBeNull();
        provider.Should().BeOfType<MockDatabaseProvider>();
    }

    [Fact]
    public void AddFlowMapper_WithBehavior_RegistersBehavior()
    {
        var services = new ServiceCollection();

        services.AddFlowMapper(builder =>
        {
            builder.AddBehavior<MockPipelineBehavior>();
        });

        var sp = services.BuildServiceProvider();
        var behavior = sp.GetService<IPipelineBehavior>();

        behavior.Should().NotBeNull();
        behavior.Should().BeOfType<MockPipelineBehavior>();
    }

    [Fact]
    public void AddFlowMapper_WithCacheProvider_RegistersCache()
    {
        var services = new ServiceCollection();

        services.AddFlowMapper(builder =>
        {
            builder.UseCacheProvider<MockCacheProvider>();
        });

        var sp = services.BuildServiceProvider();
        var cache = sp.GetService<ICacheProvider>();

        cache.Should().NotBeNull();
        cache.Should().BeOfType<MockCacheProvider>();
    }

    [Fact]
    public void AddFlowMapper_WithNamingStrategy_RegistersStrategy()
    {
        var services = new ServiceCollection();

        services.AddFlowMapper(builder =>
        {
            builder.UseNamingStrategy<MockNamingStrategy>();
        });

        var sp = services.BuildServiceProvider();
        var strategy = sp.GetService<INamingStrategy>();

        strategy.Should().NotBeNull();
        strategy.Should().BeOfType<MockNamingStrategy>();
    }

    [Fact]
    public void AddFlowMapper_WithProfile_RegistersProfile()
    {
        var services = new ServiceCollection();

        services.AddFlowMapper(builder =>
        {
            builder.AddProfile<TestProfile>();
        });

        var sp = services.BuildServiceProvider();
        var profile = sp.GetService<TestProfile>();

        profile.Should().NotBeNull();
    }

    [Fact]
    public void UseFlowMapper_InitializesExtensions()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IDatabaseProvider, MockDatabaseProvider>();
        services.AddFlowMapper();
        var sp = services.BuildServiceProvider();

        var result = sp.UseFlowMapper();

        result.Should().BeSameAs(sp);
    }

    [Fact]
    public void AddFlowMapper_CanRegisterMultipleTimes()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IDatabaseProvider, MockDatabaseProvider>();

        services.AddFlowMapper();
        services.AddFlowMapper();

        var sp = services.BuildServiceProvider();
        var mapper = sp.GetService<IRapidMapper>();

        mapper.Should().NotBeNull();
    }
}

public class MockDatabaseProvider : IDatabaseProvider
{
    public string Name => "Mock";

    public IDbConnection CreateConnection() => Mock.Of<IDbConnection>();

    public Task<IDataReader> ExecuteReaderAsync(IDbConnection connection, string sql, object? parameters, ExecutionOptions options, CancellationToken cancellationToken)
        => Task.FromResult(Mock.Of<IDataReader>());

    public Task<int> ExecuteNonQueryAsync(IDbConnection connection, string sql, object? parameters, ExecutionOptions options, CancellationToken cancellationToken)
        => Task.FromResult(0);

    public Task<object?> ExecuteScalarAsync(IDbConnection connection, string sql, object? parameters, ExecutionOptions options, CancellationToken cancellationToken)
        => Task.FromResult<object?>(null);
}

public class MockPipelineBehavior : IPipelineBehavior
{
    public Task HandleAsync<T>(ExecutionContext<T> context, Func<Task> next)
        => next();

    public bool ShouldExecute<T>(ExecutionContext<T> context) => true;
}

public class MockCacheProvider : ICacheProvider
{
    public Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
        => Task.FromResult<T?>(default);

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken ct = default)
        => Task.CompletedTask;

    public Task RemoveAsync(string key, CancellationToken ct = default)
        => Task.CompletedTask;
}

public class MockNamingStrategy : INamingStrategy
{
    public string MapColumnToProperty(string columnName) => columnName;
}

public class TestProfile : ProfileDefinition
{
    public TestProfile()
    {
        Name = "Test";
    }
}
