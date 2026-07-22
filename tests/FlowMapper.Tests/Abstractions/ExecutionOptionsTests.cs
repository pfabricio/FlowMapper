using FlowMapper.Abstractions;
using FluentAssertions;

namespace FlowMapper.Tests.Abstractions;

public class ExecutionOptionsTests
{
    [Fact]
    public void DefaultValues_AreSet()
    {
        var opts = new ExecutionOptions();

        opts.Timeout.Should().BeNull();
        opts.CommandType.Should().Be(CommandType.Text);
        opts.CancellationToken.Should().Be(default(CancellationToken));
        opts.ConnectionName.Should().BeNull();
        opts.CacheKey.Should().BeNull();
        opts.CacheExpiration.Should().BeNull();
    }

    [Fact]
    public void CanSetAllProperties()
    {
        var cts = new CancellationTokenSource();
        var expiry = TimeSpan.FromMinutes(5);

        var opts = new ExecutionOptions
        {
            Timeout = 30,
            CommandType = CommandType.StoredProcedure,
            CancellationToken = cts.Token,
            ConnectionName = "ReadOnly",
            CacheKey = "users:all",
            CacheExpiration = expiry
        };

        opts.Timeout.Should().Be(30);
        opts.CommandType.Should().Be(CommandType.StoredProcedure);
        opts.CancellationToken.Should().Be(cts.Token);
        opts.ConnectionName.Should().Be("ReadOnly");
        opts.CacheKey.Should().Be("users:all");
        opts.CacheExpiration.Should().Be(expiry);
    }
}
