using System;
using System.Linq;
using System.Reflection;
using FlowMapper.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace FlowMapper.DependencyInjection;

/// <summary>Extension methods for registering FlowMapper services in the DI container.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>Adds FlowMapper services to the DI container. Registers <c>IFlowMapper</c> and scans
    /// loaded assemblies for <c>IMapper&lt;,&gt;</c> implementations (generated at compile time).</summary>
    /// <param name="services">The <c>IServiceCollection</c> to add services to.</param>
    /// <param name="configureOptions">Optional callback to configure <c>FlowMapperOptions</c>.</param>
    public static IServiceCollection AddFlowMapper(
        this IServiceCollection services,
        Action<FlowMapperOptions>? configureOptions = null)
    {
        if (configureOptions != null)
        {
            var options = new FlowMapperOptions();
            configureOptions(options);
            services.AddSingleton(options);
        }
        else
        {
            services.AddSingleton(new FlowMapperOptions());
        }

        services.AddScoped<IFlowMapper, FlowMapperService>();

        var mapperTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a =>
            {
                try
                {
                    return a.GetTypes();
                }
                catch (ReflectionTypeLoadException)
                {
                    return Type.EmptyTypes;
                }
            })
            .Where(t => t is { IsClass: true, IsAbstract: false })
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType
                    && i.GetGenericTypeDefinition() == typeof(IMapper<,>))
                .Select(i => new { MapperType = t, ServiceType = i }))
            .ToList();

        foreach (var item in mapperTypes)
        {
            services.AddTransient(item.ServiceType, item.MapperType);
        }

        return services;
    }
}
