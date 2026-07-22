using FlowMapper.Abstractions;
using FluentAssertions;

namespace FlowMapper.Tests.Abstractions;

public class ExecutionContextTests
{
    [Fact]
    public void Constructor_SetsExecutionType()
    {
        var ctx = new ExecutionContext<object>(ExecutionType.Query);

        ctx.ExecutionType.Should().Be(ExecutionType.Query);
    }

    [Fact]
    public void DefaultValues_AreSet()
    {
        var ctx = new ExecutionContext<object>(ExecutionType.Command);

        ctx.Sql.Should().BeEmpty();
        ctx.Parameters.Should().BeNull();
        ctx.Connection.Should().BeNull();
        ctx.Result.Should().Be(default);
        ctx.Exception.Should().BeNull();
        ctx.Phase.Should().Be(ExecutionPhase.BeforeExecute);
        ctx.Metadata.Should().BeEmpty();
        ctx.Metrics.Should().NotBeNull();
        ctx.Options.Should().NotBeNull();
        ctx.ExecutionId.Should().NotBeEmpty();
    }

    [Fact]
    public void SetPhase_UpdatesPhase()
    {
        var ctx = new ExecutionContext<object>(ExecutionType.Query);

        ctx.SetPhase(ExecutionPhase.Execute);
        ctx.Phase.Should().Be(ExecutionPhase.Execute);

        ctx.SetPhase(ExecutionPhase.AfterExecute);
        ctx.Phase.Should().Be(ExecutionPhase.AfterExecute);
    }

    [Fact]
    public void ReturnType_IsCorrect()
    {
        var ctx = new ExecutionContext<string>(ExecutionType.Query);
        ctx.ReturnType.Should().Be(typeof(string));
    }

    [Fact]
    public void Metrics_StartTime_EndTime_TrackDuration()
    {
        var ctx = new ExecutionContext<object>(ExecutionType.Query);
        ctx.Metrics.StartTime = DateTime.UtcNow.AddSeconds(-1);
        ctx.Metrics.EndTime = DateTime.UtcNow;

        ctx.Metrics.TotalDuration.Should().NotBeNull();
        ctx.Metrics.TotalDuration!.Value.TotalSeconds.Should().BeApproximately(1, 0.5);
    }
}
