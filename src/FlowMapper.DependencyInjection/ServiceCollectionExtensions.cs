using FlowMapper.Abstractions;
using FlowMapper.Compiler;
using FlowMapper.Core;
using FlowMapper.Deserialization;
using FlowMapper.Diagnostics;
using FlowMapper.Diagnostics.Rules;
using FlowMapper.FullTextSearch;
using FlowMapper.Materializer;
using FlowMapper.Providers.Abstractions;
using FlowMapper.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FlowMapper.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFlowMapper(
        this IServiceCollection services,
        Action<FlowMapperBuilder>? configure = null)
    {
        var builder = new FlowMapperBuilder(services);
        configure?.Invoke(builder);
        var options = builder.GetOptions();

        services.AddSingleton(options);
        services.AddSingleton(options.Data);
        services.AddSingleton(options.Mapping);

        RegisterDataAccess(services, options.Data);
        RegisterObjectMapping(services);
        RegisterCore(services);
        RegisterFullTextSearch(services);
        RegisterDiagnostics(services);

        return services;
    }

    private static void RegisterDataAccess(IServiceCollection services, DataOptions options)
    {
        services.TryAddSingleton<IMaterializer>(sp =>
        {
            var pipeline = new Materializer.Pipeline.MaterializationPipeline(
                separator: options.CascadeSeparator);
            return new Materializer.Materializer(pipeline, options.CascadeSeparator);
        });

        services.TryAddSingleton<IConnectionFactory>(sp =>
        {
            var provider = sp.GetRequiredService<IDatabaseProvider>();
            return new ConnectionFactory(() => provider.CreateConnection());
        });

        services.AddSingleton<IExecutionScopeFactory, ExecutionScopeFactory>();

        services.TryAddSingleton<IPipelineExecutor>(sp =>
        {
            var pipelineBehaviors = sp.GetServices<IPipelineBehavior>();
            var connectionFactory = sp.GetRequiredService<IConnectionFactory>();
            var materializer = sp.GetRequiredService<IMaterializer>();
            var scopeFactory = sp.GetRequiredService<IExecutionScopeFactory>();
            return new PipelineExecutor(pipelineBehaviors, connectionFactory, materializer, scopeFactory);
        });

        services.TryAddSingleton<IQueryExecutor, QueryExecutor>();
        services.TryAddSingleton<ICommandExecutor, CommandExecutor>();
        services.TryAddSingleton<IStreamExecutor, StreamExecutor>();
        services.TryAddSingleton<IRapidMapper, RapidMapperService>();
        services.TryAddSingleton<IDeserializer, DeserializationPipeline>();
    }

    private static void RegisterObjectMapping(IServiceCollection services)
    {
        services.TryAddSingleton<IFlowMapper, FlowMapperService>();
    }

    private static void RegisterCore(IServiceCollection services)
    {
        services.TryAddSingleton<FlowBuilder>();
        services.TryAddSingleton<Compiler.ICompiler, Compiler.Compiler>();
    }

    private static void RegisterFullTextSearch(IServiceCollection services)
    {
        services.TryAddSingleton<IFullTextIndexRegistry>(sp =>
        {
            var registries = sp.GetServices<IFullTextIndexRegistry>();
            return registries.FirstOrDefault() ?? new FullTextIndexRegistry();
        });
    }

    private static void RegisterDiagnostics(IServiceCollection services)
    {
        services.TryAddSingleton<IDiagnosticTelemetry, DiagnosticTelemetry>();
        services.TryAddSingleton<IDiagnosticCollector, DiagnosticCollector>();
        services.TryAddSingleton<DiagnosticEngine>();
        services.AddSingleton<IPipelineBehavior, DiagnosticBehavior>();

        services.TryAddSingleton<ISchemaInspector, SchemaInspector>();
        services.TryAddSingleton<FlowMapperDiagnosticsOptions>();

        services.AddSingleton<IDiagnosticRule, FullTextIndexRule>();
        services.AddSingleton<IDiagnosticRule, LikeWildcardRule>();
        services.AddSingleton<IDiagnosticRule, OrderByIndexRule>();
        services.AddSingleton<IDiagnosticRule, SelectStarRule>();
        services.AddSingleton<IDiagnosticRule, LargeOffsetRule>();
        services.AddSingleton<IDiagnosticRule, CartesianJoinRule>();
    }
}
