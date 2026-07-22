using FlowMapper.Abstractions;
using FluentAssertions;

namespace FlowMapper.Tests.Abstractions;

public class ExecutionPhaseTests
{
    [Fact]
    public void EnumValues_AreOrdered()
    {
        var phases = Enum.GetValues<ExecutionPhase>();

        phases[0].Should().Be(ExecutionPhase.BeforeExecute);
        phases[1].Should().Be(ExecutionPhase.Execute);
        phases[2].Should().Be(ExecutionPhase.Mapping);
        phases[3].Should().Be(ExecutionPhase.RowRead);
        phases[4].Should().Be(ExecutionPhase.AfterExecute);
        phases[5].Should().Be(ExecutionPhase.Completed);
    }
}
