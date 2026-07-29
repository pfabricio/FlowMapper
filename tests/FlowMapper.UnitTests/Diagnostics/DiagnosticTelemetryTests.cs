using FlowMapper.Diagnostics;
using Xunit;

namespace FlowMapper.UnitTests.Diagnostics;

public class DiagnosticTelemetryTests
{
    [Fact]
    public void Record_IncrementsCount()
    {
        var telemetry = new DiagnosticTelemetry();
        telemetry.Record(new Diagnostic("FM1001", DiagnosticSeverity.Info, "test"));

        Assert.Equal(1, telemetry.GetCount("FM1001"));
    }

    [Fact]
    public void Record_MultipleSameCode_Accumulates()
    {
        var telemetry = new DiagnosticTelemetry();
        telemetry.Record(new Diagnostic("FM1001", DiagnosticSeverity.Info, "a"));
        telemetry.Record(new Diagnostic("FM1001", DiagnosticSeverity.Warning, "b"));

        Assert.Equal(2, telemetry.GetCount("FM1001"));
    }

    [Fact]
    public void Record_MultipleCodes_TracksSeparately()
    {
        var telemetry = new DiagnosticTelemetry();
        telemetry.Record(new Diagnostic("FM1001", DiagnosticSeverity.Info, "a"));
        telemetry.Record(new Diagnostic("FM3002", DiagnosticSeverity.Warning, "b"));

        Assert.Equal(1, telemetry.GetCount("FM1001"));
        Assert.Equal(1, telemetry.GetCount("FM3002"));
    }

    [Fact]
    public void GetCount_UnknownCode_ReturnsZero()
    {
        var telemetry = new DiagnosticTelemetry();
        Assert.Equal(0, telemetry.GetCount("NONEXISTENT"));
    }

    [Fact]
    public void GetAllCounts_ReturnsSnapshot()
    {
        var telemetry = new DiagnosticTelemetry();
        telemetry.Record(new Diagnostic("FM1001", DiagnosticSeverity.Info, "a"));

        var snapshot = telemetry.GetAllCounts();
        telemetry.Reset();

        Assert.Single(snapshot);
        Assert.Equal(1, snapshot["FM1001"]);
        Assert.Equal(0, telemetry.GetCount("FM1001"));
    }

    [Fact]
    public void Reset_ClearsAllCounts()
    {
        var telemetry = new DiagnosticTelemetry();
        telemetry.Record(new Diagnostic("FM1001", DiagnosticSeverity.Info, "a"));
        telemetry.Reset();

        Assert.Equal(0, telemetry.GetCount("FM1001"));
        Assert.Empty(telemetry.GetAllCounts());
    }

    [Fact]
    public void OnDiagnostic_EventFiredOnRecord()
    {
        var telemetry = new DiagnosticTelemetry();
        Diagnostic? captured = null;
        telemetry.OnDiagnostic += d => captured = d;

        var diagnostic = new Diagnostic("FM1001", DiagnosticSeverity.Info, "test");
        telemetry.Record(diagnostic);

        Assert.Same(diagnostic, captured);
    }

    [Fact]
    public void OnDiagnostic_MultipleSubscribers_AllCalled()
    {
        var telemetry = new DiagnosticTelemetry();
        var called1 = false;
        var called2 = false;
        telemetry.OnDiagnostic += _ => called1 = true;
        telemetry.OnDiagnostic += _ => called2 = true;

        telemetry.Record(new Diagnostic("FM1001", DiagnosticSeverity.Info, "test"));

        Assert.True(called1);
        Assert.True(called2);
    }
}
