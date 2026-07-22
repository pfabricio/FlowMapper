using FlowMapper.Execution;
using Xunit;

namespace FlowMapper.UnitTests;

public class MaterializerPlanTests
{
    [Fact]
    public void BuildPlan_CreatesBindingsForAllWritableProperties()
    {
        var plan = FlowMapper.Materializer.Materializer.BuildPlanFlat<TestModel>();

        Assert.NotNull(plan);
        Assert.Equal(typeof(TestModel), plan.TargetType);
        Assert.Contains(plan.Bindings, b => b.PropertyName == "Id" && b.ColumnName == "Id");
        Assert.Contains(plan.Bindings, b => b.PropertyName == "Name" && b.ColumnName == "Name");
    }
}

public class TestModel
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}
