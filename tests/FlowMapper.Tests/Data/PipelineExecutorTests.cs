using FlowMapper.Abstractions;
using FlowMapper.Data.Pipeline;
using FluentAssertions;
using Moq;

namespace FlowMapper.Tests.Data;

public class PipelineExecutorTests
{
    [Fact]
    public void Constructor_WithNoBehaviors_DoesNotThrow()
    {
        var provider = Mock.Of<IDatabaseProvider>();
        var mapper = Mock.Of<IDataReaderMapper>();
        var scopeFactory = Mock.Of<IExecutionScopeFactory>();
        var strategy = Mock.Of<IExecutionStrategy>();

        var act = () => new PipelineExecutor(
            Array.Empty<IPipelineBehavior>(),
            provider, mapper, scopeFactory, strategy);

        act.Should().NotThrow();
    }

    [Fact]
    public void Constructor_WithAllDependencies_DoesNotThrow()
    {
        var act = () => new PipelineExecutor(
            new[] { Mock.Of<IPipelineBehavior>() },
            Mock.Of<IDatabaseProvider>(),
            Mock.Of<IDataReaderMapper>(),
            Mock.Of<IExecutionScopeFactory>(),
            Mock.Of<IExecutionStrategy>(),
            Mock.Of<ICacheProvider>());

        act.Should().NotThrow();
    }

    [Fact]
    public void Constructor_WithNullBehaviors_DoesNotThrow()
    {
        var act = () => new PipelineExecutor(
            null!,
            Mock.Of<IDatabaseProvider>(),
            Mock.Of<IDataReaderMapper>(),
            Mock.Of<IExecutionScopeFactory>(),
            Mock.Of<IExecutionStrategy>());

        act.Should().NotThrow();
    }

    [Fact]
    public async Task ExecuteAsync_QueryWithoutCache_ExecutesPipeline()
    {
        var scopeMock = new Mock<IExecutionScope>();
        scopeMock.Setup(s => s.Connection).Returns(Mock.Of<IDbConnection>());
        scopeMock.Setup(s => s.CommitAsync()).Returns(Task.CompletedTask);

        var scopeFactoryMock = new Mock<IExecutionScopeFactory>();
        scopeFactoryMock.Setup(f => f.CreateScope(It.IsAny<string?>(), false))
            .Returns(scopeMock.Object);

        var providerMock = new Mock<IDatabaseProvider>();
        providerMock.Setup(p => p.Name).Returns("TestProvider");
        providerMock.Setup(p => p.ExecuteReaderAsync(
            It.IsAny<IDbConnection>(),
            It.IsAny<string>(),
            It.IsAny<object?>(),
            It.IsAny<ExecutionOptions>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Mock.Of<IDataReader>());

        var dataMapperMock = new Mock<IDataReaderMapper>();
        dataMapperMock.Setup(m => m.Map<string>(
            It.IsAny<IDataReader>(),
            It.IsAny<FlowMapper.Abstractions.MappingOptions>()))
            .Returns(new[] { "result1", "result2" });

        var strategyMock = new Mock<IExecutionStrategy>();
        strategyMock.Setup(s => s.ExecuteAsync(It.IsAny<Func<Task>>()))
            .Returns<Func<Task>>(fn => fn());

        var executor = new PipelineExecutor(
            Array.Empty<IPipelineBehavior>(),
            providerMock.Object,
            dataMapperMock.Object,
            scopeFactoryMock.Object,
            strategyMock.Object);

        var context = new ExecutionContext<string>(ExecutionType.Query)
        {
            Sql = "SELECT * FROM Test",
            Options = new ExecutionOptions()
        };

        await executor.ExecuteAsync(context);

        context.Exception.Should().BeNull();
        context.Phase.Should().Be(ExecutionPhase.Completed);
        providerMock.Verify(p => p.ExecuteReaderAsync(
            It.IsAny<IDbConnection>(), "SELECT * FROM Test", null,
            It.IsAny<ExecutionOptions>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithCacheHit_ReturnsCachedResult()
    {
        var cacheMock = new Mock<ICacheProvider>();
        cacheMock.Setup(c => c.GetAsync<string>(
            "my-cache-key", It.IsAny<CancellationToken>()))
            .ReturnsAsync("cached-value");

        var executor = new PipelineExecutor(
            Array.Empty<IPipelineBehavior>(),
            Mock.Of<IDatabaseProvider>(),
            Mock.Of<IDataReaderMapper>(),
            Mock.Of<IExecutionScopeFactory>(),
            Mock.Of<IExecutionStrategy>(),
            cacheMock.Object);

        var context = new ExecutionContext<string>(ExecutionType.Query)
        {
            Sql = "SELECT * FROM Test",
            Options = new ExecutionOptions { CacheKey = "my-cache-key" }
        };

        await executor.ExecuteAsync(context);

        context.Result.Should().Be("cached-value");
        context.Exception.Should().BeNull();
    }

    [Fact]
    public async Task ExecuteAsync_Command_ExecutesNonQuery()
    {
        var scopeMock = new Mock<IExecutionScope>();
        scopeMock.Setup(s => s.Connection).Returns(Mock.Of<IDbConnection>());
        scopeMock.Setup(s => s.CommitAsync()).Returns(Task.CompletedTask);

        var scopeFactoryMock = new Mock<IExecutionScopeFactory>();
        scopeFactoryMock.Setup(f => f.CreateScope(It.IsAny<string?>(), true))
            .Returns(scopeMock.Object);

        var providerMock = new Mock<IDatabaseProvider>();
        providerMock.Setup(p => p.Name).Returns("TestProvider");
        providerMock.Setup(p => p.ExecuteNonQueryAsync(
            It.IsAny<IDbConnection>(),
            It.IsAny<string>(),
            It.IsAny<object?>(),
            It.IsAny<ExecutionOptions>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(42);

        var strategyMock = new Mock<IExecutionStrategy>();
        strategyMock.Setup(s => s.ExecuteAsync(It.IsAny<Func<Task>>()))
            .Returns<Func<Task>>(fn => fn());

        var executor = new PipelineExecutor(
            Array.Empty<IPipelineBehavior>(),
            providerMock.Object,
            Mock.Of<IDataReaderMapper>(),
            scopeFactoryMock.Object,
            strategyMock.Object);

        var context = new ExecutionContext<int>(ExecutionType.Command)
        {
            Sql = "DELETE FROM Test",
            Options = new ExecutionOptions()
        };

        await executor.ExecuteAsync(context);

        context.Result.Should().Be(42);
        context.Exception.Should().BeNull();
        providerMock.Verify(p => p.ExecuteNonQueryAsync(
            It.IsAny<IDbConnection>(), "DELETE FROM Test", null,
            It.IsAny<ExecutionOptions>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenExceptionThrown_PropagatesAndRollsBack()
    {
        var scopeMock = new Mock<IExecutionScope>();
        scopeMock.Setup(s => s.Connection).Returns(Mock.Of<IDbConnection>());
        scopeMock.Setup(s => s.RollbackAsync()).Returns(Task.CompletedTask);

        var scopeFactoryMock = new Mock<IExecutionScopeFactory>();
        scopeFactoryMock.Setup(f => f.CreateScope(It.IsAny<string?>(), false))
            .Returns(scopeMock.Object);

        var providerMock = new Mock<IDatabaseProvider>();
        providerMock.Setup(p => p.Name).Returns("TestProvider");
        providerMock.Setup(p => p.ExecuteReaderAsync(
            It.IsAny<IDbConnection>(),
            It.IsAny<string>(),
            It.IsAny<object?>(),
            It.IsAny<ExecutionOptions>(),
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("DB failure"));

        var strategyMock = new Mock<IExecutionStrategy>();
        strategyMock.Setup(s => s.ExecuteAsync(It.IsAny<Func<Task>>()))
            .Returns<Func<Task>>(fn => fn());

        var executor = new PipelineExecutor(
            Array.Empty<IPipelineBehavior>(),
            providerMock.Object,
            Mock.Of<IDataReaderMapper>(),
            scopeFactoryMock.Object,
            strategyMock.Object);

        var context = new ExecutionContext<string>(ExecutionType.Query)
        {
            Sql = "SELECT * FROM Fail",
            Options = new ExecutionOptions()
        };

        var act = async () => await executor.ExecuteAsync(context);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("DB failure");

        scopeMock.Verify(s => s.RollbackAsync(), Times.Once);
    }

    [Fact]
    public void ExecuteAsync_WithNullStrategy_Throws()
    {
        var executor = new PipelineExecutor(
            Array.Empty<IPipelineBehavior>(),
            Mock.Of<IDatabaseProvider>(),
            Mock.Of<IDataReaderMapper>(),
            Mock.Of<IExecutionScopeFactory>(),
            null!);

        var context = new ExecutionContext<string>(ExecutionType.Query)
        {
            Sql = "SELECT 1",
            Options = new ExecutionOptions()
        };

        var act = async () => await executor.ExecuteAsync(context);

        act.Should().ThrowAsync<NullReferenceException>();
    }
}
