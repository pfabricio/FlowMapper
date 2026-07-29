using FlowMapper.Abstractions;
using FlowMapper.Diagnostics;
using Xunit;

namespace FlowMapper.UnitTests.Diagnostics;

public class DiagnosticEngineTests
{
    [Fact]
    public void Analyze_EligibleRule_EmitDiagnostics()
    {
        var rule = new MockRule(true, [new Diagnostic("TST01", DiagnosticSeverity.Warning, "mock")]);
        var collector = new DiagnosticCollector();
        var engine = new DiagnosticEngine([rule], collector);

        engine.Analyze(new QueryContext { Sql = "SELECT * FROM T" });

        Assert.Single(collector.Diagnostics);
        Assert.Equal("TST01", collector.Diagnostics[0].Code);
    }

    [Fact]
    public void Analyze_IneligibleRule_Skips()
    {
        var rule = new MockRule(false, [new Diagnostic("TST01", DiagnosticSeverity.Warning, "mock")]);
        var collector = new DiagnosticCollector();
        var engine = new DiagnosticEngine([rule], collector);

        engine.Analyze(new QueryContext { Sql = "SELECT * FROM T" });

        Assert.Empty(collector.Diagnostics);
    }

    [Fact]
    public void Analyze_MultipleEligibleRules_EmitsAll()
    {
        var rule1 = new MockRule(true, [new Diagnostic("R01", DiagnosticSeverity.Info, "r1")]);
        var rule2 = new MockRule(true, [new Diagnostic("R02", DiagnosticSeverity.Warning, "r2")]);
        var collector = new DiagnosticCollector();
        var engine = new DiagnosticEngine([rule1, rule2], collector);

        engine.Analyze(new QueryContext { Sql = "SELECT * FROM T" });

        Assert.Equal(2, collector.Diagnostics.Count);
    }

    [Fact]
    public void Analyze_NoRules_DoesNotThrow()
    {
        var collector = new DiagnosticCollector();
        var engine = new DiagnosticEngine([], collector);

        engine.Analyze(new QueryContext { Sql = "SELECT * FROM T" });

        Assert.Empty(collector.Diagnostics);
    }

    private class MockRule(bool canAnalyze, IEnumerable<Diagnostic> diagnostics) : IDiagnosticRule
    {
        public bool CanAnalyze(QueryContext context) => canAnalyze;
        public IEnumerable<Diagnostic> Analyze(QueryContext context) => diagnostics;
    }
}
