using FlowMapper.Diagnostics;
using Xunit;

namespace FlowMapper.UnitTests.Diagnostics;

public class DiagnosticCollectorTests
{
    [Fact]
    public void Emit_StoresDiagnostic()
    {
        var collector = new DiagnosticCollector();
        var diagnostic = new Diagnostic("TST01", DiagnosticSeverity.Info, "test");

        collector.Emit(diagnostic);

        Assert.Single(collector.Diagnostics);
        Assert.Same(diagnostic, collector.Diagnostics[0]);
    }

    [Fact]
    public void HasErrors_NoDiagnostics_ReturnsFalse()
    {
        var collector = new DiagnosticCollector();
        Assert.False(collector.HasErrors);
    }

    [Fact]
    public void HasErrors_WithErrorDiagnostic_ReturnsTrue()
    {
        var collector = new DiagnosticCollector();
        collector.Emit(new Diagnostic("ERR01", DiagnosticSeverity.Error, "error"));
        Assert.True(collector.HasErrors);
    }

    [Fact]
    public void HasErrors_WithInfoOnly_ReturnsFalse()
    {
        var collector = new DiagnosticCollector();
        collector.Emit(new Diagnostic("INF01", DiagnosticSeverity.Info, "info"));
        collector.Emit(new Diagnostic("WRN01", DiagnosticSeverity.Warning, "warning"));
        Assert.False(collector.HasErrors);
    }

    [Fact]
    public void Clear_RemovesAllDiagnostics()
    {
        var collector = new DiagnosticCollector();
        collector.Emit(new Diagnostic("TST01", DiagnosticSeverity.Info, "test"));
        collector.Clear();
        Assert.Empty(collector.Diagnostics);
        Assert.False(collector.HasErrors);
    }

    [Fact]
    public void Diagnostics_ReturnsSnapshot()
    {
        var collector = new DiagnosticCollector();
        collector.Emit(new Diagnostic("TST01", DiagnosticSeverity.Info, "test"));

        var snapshot = collector.Diagnostics;
        collector.Clear();

        Assert.NotEmpty(snapshot);
        Assert.Empty(collector.Diagnostics);
    }

    [Fact]
    public void Emit_WithTelemetry_InvokesRecord()
    {
        var telemetry = new DiagnosticTelemetry();
        var collector = new DiagnosticCollector(telemetry);
        var diagnostic = new Diagnostic("TST01", DiagnosticSeverity.Info, "test");

        collector.Emit(diagnostic);

        Assert.Equal(1, telemetry.GetCount("TST01"));
    }

    [Fact]
    public void Emit_WithTelemetry_FiresOnDiagnostic()
    {
        var telemetry = new DiagnosticTelemetry();
        var collector = new DiagnosticCollector(telemetry);
        Diagnostic? captured = null;
        telemetry.OnDiagnostic += d => captured = d;

        var diagnostic = new Diagnostic("TST01", DiagnosticSeverity.Info, "test");
        collector.Emit(diagnostic);

        Assert.Same(diagnostic, captured);
    }
}
