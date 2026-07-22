using FlowMapper.Abstractions;
using Xunit;

namespace FlowMapper.UnitTests;

public class ExecutionOptionsTests
{
    [Fact]
    public void DefaultTimeout_IsNull()
    {
        var options = new ExecutionOptions();
        Assert.Null(options.Timeout);
    }

    [Fact]
    public void DefaultCommandType_IsText()
    {
        var options = new ExecutionOptions();
        Assert.Equal(System.Data.CommandType.Text, options.CommandType);
    }
}
