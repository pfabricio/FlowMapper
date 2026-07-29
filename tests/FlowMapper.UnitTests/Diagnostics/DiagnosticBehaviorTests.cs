using System.Data;
using FlowMapper.Abstractions;
using FlowMapper.Diagnostics;
using Xunit;

namespace FlowMapper.UnitTests.Diagnostics;

public class DiagnosticBehaviorTests
{
    [Fact]
    public async Task HandleAsync_AnalyzesContextAndCallsNext()
    {
        var collector = new DiagnosticCollector();
        var rule = new AlwaysTrueRule([new Diagnostic("TST01", DiagnosticSeverity.Info, "test")]);
        var engine = new DiagnosticEngine([rule], collector);
        var provider = new SpyProvider("SqlServer");
        var behavior = new DiagnosticBehavior(engine, provider);

        var context = new ExecutionContext<string>("SELECT * FROM T", null, null);
        var nextCalled = false;

        await behavior.HandleAsync(context, () =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        Assert.True(nextCalled);
        Assert.NotEmpty(collector.Diagnostics);
    }

    [Fact]
    public void ShouldExecute_AlwaysTrue()
    {
        var collector = new DiagnosticCollector();
        var engine = new DiagnosticEngine([], collector);
        var provider = new SpyProvider("SqlServer");
        var behavior = new DiagnosticBehavior(engine, provider);

        Assert.True(behavior.ShouldExecute(new ExecutionContext<string>("SELECT 1", null, null)));
    }

    private class AlwaysTrueRule(IEnumerable<Diagnostic> diagnostics) : IDiagnosticRule
    {
        public bool CanAnalyze(QueryContext context) => true;
        public IEnumerable<Diagnostic> Analyze(QueryContext context) => diagnostics;
    }

    private class SpyProvider(string name) : IDatabaseProvider
    {
        public string Name => name;
        public Version Version => new(1, 0);
        public IDialect Dialect => throw new NotImplementedException();
        public IDbConnection CreateConnection() => throw new NotImplementedException();
        public IDbCommand CreateCommand(string sql, IDbConnection connection, IDbTransaction? transaction) => throw new NotImplementedException();
        public IDataParameter CreateParameter(string name, object? value) => throw new NotImplementedException();
    }
}
