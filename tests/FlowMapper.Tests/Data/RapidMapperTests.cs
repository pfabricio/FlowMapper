using FlowMapper.Abstractions;
using FlowMapper.Data;
using FlowMapper.Data.Mapping;
using FluentAssertions;
using Moq;

namespace FlowMapper.Tests.Data;

public class RapidMapperTests
{
    [Fact]
    public void Constructor_RequiresExecutors()
    {
        var query = Mock.Of<IQueryExecutor>();
        var command = Mock.Of<ICommandExecutor>();
        var stream = Mock.Of<IStreamExecutor>();
        var scopeFactory = Mock.Of<IExecutionScopeFactory>();

        var mapper = new RapidMapper(query, command, stream, scopeFactory);

        mapper.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_ThrowsOnNullQuery()
    {
        var act = () => new RapidMapper(null!, Mock.Of<ICommandExecutor>(), Mock.Of<IStreamExecutor>(), Mock.Of<IExecutionScopeFactory>());

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task QueryAsync_DelegatesToQueryExecutor()
    {
        var queryMock = new Mock<IQueryExecutor>();
        queryMock.Setup(q => q.QueryAsync<object>("SELECT 1", null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<object>());

        var mapper = new RapidMapper(queryMock.Object, Mock.Of<ICommandExecutor>(), Mock.Of<IStreamExecutor>(), Mock.Of<IExecutionScopeFactory>());
        var result = await mapper.QueryAsync<object>("SELECT 1");

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task ExecuteAsync_DelegatesToCommandExecutor()
    {
        var commandMock = new Mock<ICommandExecutor>();
        commandMock.Setup(c => c.ExecuteAsync("DELETE FROM T", null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(5);

        var mapper = new RapidMapper(Mock.Of<IQueryExecutor>(), commandMock.Object, Mock.Of<IStreamExecutor>(), Mock.Of<IExecutionScopeFactory>());
        var result = await mapper.ExecuteAsync("DELETE FROM T");

        result.Should().Be(5);
    }

    [Fact]
    public void CreateScope_DelegatesToFactory()
    {
        var factoryMock = new Mock<IExecutionScopeFactory>();
        var scopeMock = new Mock<IExecutionScope>();
        factoryMock.Setup(f => f.CreateScope("Default", true)).Returns(scopeMock.Object);

        var mapper = new RapidMapper(Mock.Of<IQueryExecutor>(), Mock.Of<ICommandExecutor>(), Mock.Of<IStreamExecutor>(), factoryMock.Object);
        var scope = mapper.CreateScope("Default", true);

        scope.Should().Be(scopeMock.Object);
    }
}
