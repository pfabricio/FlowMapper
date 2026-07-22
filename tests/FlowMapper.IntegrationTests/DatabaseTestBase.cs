using FlowMapper.Abstractions;
using FlowMapper.Core;
using FlowMapper.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FlowMapper.IntegrationTests;

public abstract class DatabaseTestBase : IAsyncLifetime
{
    protected IServiceProvider ServiceProvider { get; private set; } = null!;
    protected IRapidMapper Rapid { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await StartContainer();

        var services = new ServiceCollection();

        services.AddFlowMapper(builder =>
        {
            builder.AddProfile<TestProfile>();
            ConfigureProvider(builder);
        });

        ServiceProvider = services.BuildServiceProvider();

        DataMapperExtensions.Initialize(ServiceProvider);

        Rapid = ServiceProvider.GetRequiredService<IRapidMapper>();

        await SeedDatabase();
    }

    protected abstract Task StartContainer();
    protected abstract void ConfigureProvider(FlowMapperBuilder builder);
    protected abstract Task SeedDatabase();

    public abstract Task DisposeAsync();
}

public class TestProfile : ProfileDefinition
{
    public TestProfile()
    {
        ProfileName = "TestProfile";

        CreateMap<TestEntity, TestDto>()
            .ForMember(d => d.FullName, opt => opt.MapFrom(s => $"{s.FirstName} {s.LastName}"));
    }
}

public class TestEntity
{
    public int Id { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Email { get; set; } = "";
}

public class TestDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
}
