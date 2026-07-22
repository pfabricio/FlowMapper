using FlowMapper.Diagnostics;
using FlowMapper.Diagnostics.Pipeline;
using FlowMapper.Diagnostics.Pipeline.Middlewares;
using Xunit;

namespace FlowMapper.UnitTests;

public class DiagnosticsPipelineTests
{
    [Fact]
    public void Emit_StoresEvent()
    {
        var pipeline = new DiagnosticsPipeline();

        pipeline.Emit(new DiagnosticEvent
        {
            Category = "Test",
            Message = "test message",
            Severity = DiagnosticSeverity.Info
        });

        var events = pipeline.GetEvents();
        Assert.Single(events);
        Assert.Equal("Test", events[0].Category);
    }

    [Fact]
    public void Emit_FiltersByCategory()
    {
        var pipeline = new DiagnosticsPipeline();

        pipeline.Emit(new DiagnosticEvent { Category = "A", Message = "m1", Severity = DiagnosticSeverity.Info });
        pipeline.Emit(new DiagnosticEvent { Category = "B", Message = "m2", Severity = DiagnosticSeverity.Warning });

        var categoryA = pipeline.GetEvents("A");
        var categoryB = pipeline.GetEvents("B");

        Assert.Single(categoryA);
        Assert.Single(categoryB);
    }

    [Fact]
    public void BeginScope_TracksDuration()
    {
        var pipeline = new DiagnosticsPipeline();

        using (pipeline.BeginScope("TestScope"))
        {
            // simulate work
        }

        var events = pipeline.GetEvents("TestScope");
        Assert.Equal(2, events.Count); // start + end
        Assert.Contains(events, e => e.Message == "Scope started");
        Assert.Contains(events, e => e.Message == "Scope completed");
    }

    [Fact]
    public void LoggingMiddleware_DoesNotBlockEvents()
    {
        var logged = new List<string>();
        var pipeline = new DiagnosticsPipeline(
            [new LoggingMiddleware(msg => logged.Add(msg))]);

        pipeline.Emit(new DiagnosticEvent
        {
            Category = "Test",
            Message = "hello",
            Severity = DiagnosticSeverity.Info
        });

        Assert.Single(logged);
        Assert.Contains("hello", logged[0]);
        Assert.Single(pipeline.GetEvents());
    }

    [Fact]
    public void ThresholdMiddleware_LimitsEvents()
    {
        var pipeline = new DiagnosticsPipeline(
            [new ThresholdMiddleware(2)]);

        pipeline.Emit(new DiagnosticEvent { Category = "T", Message = "1", Severity = DiagnosticSeverity.Info });
        pipeline.Emit(new DiagnosticEvent { Category = "T", Message = "2", Severity = DiagnosticSeverity.Info });
        pipeline.Emit(new DiagnosticEvent { Category = "T", Message = "3", Severity = DiagnosticSeverity.Info });

        Assert.Equal(2, pipeline.GetEvents().Count);
    }

    [Fact]
    public void MetricsMiddleware_TracksCounts()
    {
        var metrics = new MetricsMiddleware();
        var pipeline = new DiagnosticsPipeline([metrics]);

        pipeline.Emit(new DiagnosticEvent { Category = "T", Message = "info", Severity = DiagnosticSeverity.Info });
        pipeline.Emit(new DiagnosticEvent { Category = "T", Message = "warn", Severity = DiagnosticSeverity.Warning });
        pipeline.Emit(new DiagnosticEvent { Category = "T", Message = "err", Severity = DiagnosticSeverity.Error });

        Assert.Equal(3, metrics.TotalEvents);
        Assert.Equal(1, metrics.ErrorCount);
        Assert.Equal(1, metrics.WarningCount);
    }

    [Fact]
    public void OnEvent_FiresOnEmit()
    {
        var pipeline = new DiagnosticsPipeline();
        var fired = false;

        pipeline.OnEvent += (_, _) => fired = true;
        pipeline.Emit(new DiagnosticEvent { Category = "T", Message = "test", Severity = DiagnosticSeverity.Info });

        Assert.True(fired);
    }
}
