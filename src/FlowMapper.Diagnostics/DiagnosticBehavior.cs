using FlowMapper.Abstractions;

namespace FlowMapper.Diagnostics;

public class DiagnosticBehavior : IPipelineBehavior
{
    private readonly DiagnosticEngine _engine;
    private readonly IDatabaseProvider _provider;

    public DiagnosticBehavior(DiagnosticEngine engine, IDatabaseProvider provider)
    {
        _engine = engine;
        _provider = provider;
    }

    public bool ShouldExecute<T>(ExecutionContext<T> context) => true;

    public async Task HandleAsync<T>(ExecutionContext<T> context, Func<Task> next)
    {
        var queryContext = new QueryContext
        {
            Sql = context.Sql,
            Parameters = context.Parameters,
            Provider = _provider
        };

        _engine.Analyze(queryContext);

        await next();
    }
}
