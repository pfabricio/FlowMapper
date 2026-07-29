using FlowMapper.Abstractions;
using FlowMapper.FullTextSearch;

namespace FlowMapper.Diagnostics.Rules;

public class FullTextIndexRule : IDiagnosticRule
{
    private readonly IFullTextIndexRegistry _registry;
    private readonly ISchemaInspector _schemaInspector;
    private readonly FlowMapperDiagnosticsOptions _options;

    public FullTextIndexRule(IFullTextIndexRegistry registry)
        : this(registry, new SchemaInspector(), new FlowMapperDiagnosticsOptions())
    {
    }

    public FullTextIndexRule(
        IFullTextIndexRegistry registry,
        ISchemaInspector schemaInspector,
        FlowMapperDiagnosticsOptions options)
    {
        _registry = registry;
        _schemaInspector = schemaInspector;
        _options = options;
    }

    public bool CanAnalyze(QueryContext context) => true;

    public IEnumerable<Diagnostic> Analyze(QueryContext context)
    {
        var provider = context.Provider;
        if (provider is null)
            yield break;

        var configured = _registry.GetAllConfigured();

        foreach (var (table, column) in configured)
        {
            if (!context.Sql.Contains(table, StringComparison.OrdinalIgnoreCase))
                continue;

            FtsIndexState state;

            if (_options.EnableSchemaInspection)
            {
                state = _schemaInspector.VerifyIndex(table, column, provider);
            }
            else
            {
                state = _registry.GetState(table, column);
            }

            switch (state)
            {
                case FtsIndexState.Verified:
                    yield return new Diagnostic("FM1001", DiagnosticSeverity.Info,
                        $"FTS index verified for {table}.{column}.",
                        table, column, provider.Name);
                    break;

                case FtsIndexState.Missing:
                    yield return new Diagnostic("FM1001", DiagnosticSeverity.Warning,
                        $"FTS index missing for {table}.{column}. " +
                        (provider.Dialect.FtsIndexErrorMessage ?? "Create the required FTS index."),
                        table, column, provider.Name);
                    break;

                case FtsIndexState.Configured:
                    yield return new Diagnostic("FM1001", DiagnosticSeverity.Info,
                        $"FTS index configured for {table}.{column} via profile.",
                        table, column, provider.Name);
                    break;
            }
        }
    }
}
