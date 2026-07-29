using System.Data;
using FlowMapper.Abstractions;
using FlowMapper.Diagnostics;
using FlowMapper.Execution;
// using FlowMapper.Execution.Artifacts;
using FlowMapper.Materializer;
using FlowMapper.Providers.Abstractions;
using FlowMapper.Runtime;
using Xunit;

namespace FlowMapper.UnitTests.Runtime;

public class PipelineExecutorBehaviorTests
{
    [Fact]
    public async Task QueryAsync_WithSingleBehavior_InvokesChain()
    {
        var log = new List<string>();
        var behavior = new SpyBehavior(log, "B1");
        var executor = CreateExecutor([behavior]);

        await executor.QueryAsync<string>("SELECT 1");

        Assert.Equal(["B1:ShouldExecute", "B1:HandleAsync"], log);
    }

    [Fact]
    public async Task QueryAsync_WithMultipleBehaviors_InvokesInOrder()
    {
        var log = new List<string>();
        var b1 = new SpyBehavior(log, "B1");
        var b2 = new SpyBehavior(log, "B2");
        var executor = CreateExecutor([b1, b2]);

        await executor.QueryAsync<string>("SELECT 1");

        Assert.Equal([
            "B1:ShouldExecute", "B1:HandleAsync",
            "B2:ShouldExecute", "B2:HandleAsync"
        ], log);
    }

    [Fact]
    public async Task QueryAsync_BehaviorShouldExecuteFalse_SkipsHandleAsync()
    {
        var log = new List<string>();
        var behavior = new SpyBehavior(log, "B1", shouldExecute: false);
        var executor = CreateExecutor([behavior]);

        await executor.QueryAsync<string>("SELECT 1");

        Assert.Equal(["B1:ShouldExecute"], log);
    }

    [Fact]
    public async Task QueryAsync_CoreExecutionSetsResult()
    {
        var log = new List<string>();
        var behavior = new SpyBehavior(log, "B1");
        var executor = CreateExecutor([behavior]);

        var results = await executor.QueryAsync<string>("SELECT 1");

        Assert.Equal(["hello"], results);
        Assert.Equal(["B1:ShouldExecute", "B1:HandleAsync"], log);
    }

    [Fact]
    public async Task ExecuteAsync_WithBehavior_InvokesChain()
    {
        var log = new List<string>();
        var behavior = new SpyBehavior(log, "B1");
        var executor = CreateExecutor([behavior]);

        var result = await executor.ExecuteAsync("DELETE FROM T");

        Assert.Equal(1, result);
        Assert.Equal(["B1:ShouldExecute", "B1:HandleAsync"], log);
    }

    [Fact]
    public async Task QueryAsync_DiagnosticBehavior_EmitsDiagnostics()
    {
        var collector = new DiagnosticCollector();
        var rule = new MockRule(true, [new Diagnostic("TST01", DiagnosticSeverity.Info, "test")]);
        var engine = new DiagnosticEngine([rule], collector);
        var provider = new StubProvider();
        var diagnosticBehavior = new DiagnosticBehavior(engine, provider);

        var executor = CreateExecutor([diagnosticBehavior], provider);

        await executor.QueryAsync<string>("SELECT * FROM T");

        Assert.NotEmpty(collector.Diagnostics);
        Assert.Equal("TST01", collector.Diagnostics[0].Code);
    }

    private static PipelineExecutor CreateExecutor(
        IEnumerable<IPipelineBehavior> behaviors,
        IDatabaseProvider? provider = null)
    {
        var connFactory = new StubConnectionFactory();
        var materializer = new StubMaterializer();
        var scopeFactory = new StubScopeFactory();
        return new PipelineExecutor(behaviors, connFactory, materializer, scopeFactory);
    }

    private class SpyBehavior(List<string> log, string name, bool shouldExecute = true) : IPipelineBehavior
    {
        public bool ShouldExecute<T>(ExecutionContext<T> context)
        {
            log.Add($"{name}:ShouldExecute");
            return shouldExecute;
        }

        public async Task HandleAsync<T>(ExecutionContext<T> context, Func<Task> next)
        {
            log.Add($"{name}:HandleAsync");
            await next();
        }
    }

    private class MockRule(bool canAnalyze, IEnumerable<Diagnostic> diagnostics) : IDiagnosticRule
    {
        public bool CanAnalyze(QueryContext context) => canAnalyze;
        public IEnumerable<Diagnostic> Analyze(QueryContext context) => diagnostics;
    }

    private class StubProvider : IDatabaseProvider
    {
        public string Name => "Stub";
        public Version Version => new(1, 0);
        public IDialect Dialect => throw new NotImplementedException();
        public IDbConnection CreateConnection() => throw new NotImplementedException();
        public IDbCommand CreateCommand(string sql, IDbConnection connection, IDbTransaction? transaction) => throw new NotImplementedException();
        public IDataParameter CreateParameter(string name, object? value) => throw new NotImplementedException();
    }

    private class StubConnectionFactory : IConnectionFactory
    {
        public IDbConnection CreateConnection(string? name = null) => new StubConnection();
    }

    private class StubConnection : IDbConnection
    {
        public string ConnectionString { get; set; } = "";
        public int ConnectionTimeout => 30;
        public string Database => "test";
        public ConnectionState State => ConnectionState.Open;

        public IDbTransaction BeginTransaction() => throw new NotImplementedException();
        public IDbTransaction BeginTransaction(IsolationLevel il) => throw new NotImplementedException();
        public void ChangeDatabase(string databaseName) { }
        public void Close() { }
        public IDbCommand CreateCommand() => new StubCommand();
        public void Dispose() { }
        public void Open() { }
    }

    private class StubCommand : IDbCommand
    {
        public string CommandText { get; set; } = "";
        public int CommandTimeout { get; set; }
        public System.Data.CommandType CommandType { get; set; }
        public IDbConnection? Connection { get; set; }
        public IDataParameterCollection Parameters => throw new NotImplementedException();
        public IDbTransaction? Transaction { get; set; }
        public UpdateRowSource UpdatedRowSource { get; set; }

        public void Cancel() { }
        public IDbDataParameter CreateParameter() => throw new NotImplementedException();
        public void Dispose() { }
        public int ExecuteNonQuery() => 1;
        public IDataReader ExecuteReader() => new StubReader();
        public IDataReader ExecuteReader(CommandBehavior behavior) => new StubReader();
        public object? ExecuteScalar() => null;
        public void Prepare() { }

        public Task<int> ExecuteNonQueryAsync(CancellationToken ct) => Task.FromResult(1);
        public Task<IDataReader> ExecuteReaderAsync(CancellationToken ct)
            => Task.FromResult<IDataReader>(new StubReader());
        public Task<IDataReader> ExecuteReaderAsync(CommandBehavior behavior, CancellationToken ct)
            => Task.FromResult<IDataReader>(new StubReader());
    }

    private class StubReader : IDataReader
    {
        private int _readCount;

        public int FieldCount => 1;
        public object this[int i] => "hello";
        public object this[string name] => "hello";
        public int Depth => 0;
        public bool IsClosed => false;
        public int RecordsAffected => 1;

        public void Close() { }
        public void Dispose() { }
        public bool GetBoolean(int i) => throw new NotImplementedException();
        public byte GetByte(int i) => throw new NotImplementedException();
        public long GetBytes(int i, long fieldOffset, byte[]? buffer, int bufferoffset, int length) => throw new NotImplementedException();
        public char GetChar(int i) => throw new NotImplementedException();
        public long GetChars(int i, long fieldoffset, char[]? buffer, int bufferoffset, int length) => throw new NotImplementedException();
        public IDataReader GetData(int i) => throw new NotImplementedException();
        public string GetDataTypeName(int i) => throw new NotImplementedException();
        public DateTime GetDateTime(int i) => throw new NotImplementedException();
        public decimal GetDecimal(int i) => throw new NotImplementedException();
        public double GetDouble(int i) => throw new NotImplementedException();
        public Type GetFieldType(int i) => typeof(string);
        public float GetFloat(int i) => throw new NotImplementedException();
        public Guid GetGuid(int i) => throw new NotImplementedException();
        public short GetInt16(int i) => throw new NotImplementedException();
        public int GetInt32(int i) => throw new NotImplementedException();
        public long GetInt64(int i) => throw new NotImplementedException();
        public string GetName(int i) => "Value";
        public int GetOrdinal(string name) => 0;
        public DataTable? GetSchemaTable() => throw new NotImplementedException();
        public string GetString(int i) => "hello";
        public object GetValue(int i) => "hello";
        public int GetValues(object[] values) { values[0] = "hello"; return 1; }
        public bool IsDBNull(int i) => false;
        public bool NextResult() => false;
        public bool Read()
        {
            if (_readCount >= 1) return false;
            _readCount++;
            return true;
        }

        public Task<bool> ReadAsync(CancellationToken ct) => Task.FromResult(Read());
        public Task<bool> NextResultAsync(CancellationToken ct) => Task.FromResult(false);
    }

    private class StubMaterializer : IMaterializer
    {
        public MaterializationPlan BuildPlan<T>() => new();

        public T Materialize<T>(IDataReader reader, MaterializationPlan plan)
        {
            return (T)reader.GetValue(0);
        }

        public T Materialize<T>(IDataReader reader, FlowMapper.Execution.Artifacts.IMaterializationArtifact artifact) => throw new NotImplementedException();
        public IEnumerable<T> MaterializeAll<T>(IDataReader reader, MaterializationPlan plan) => throw new NotImplementedException();
        public IEnumerable<T> MaterializeAll<T>(IDataReader reader, FlowMapper.Execution.Artifacts.IMaterializationArtifact artifact) => throw new NotImplementedException();
    }

    private class StubScopeFactory : IExecutionScopeFactory
    {
        public IExecutionScope CreateScope(bool transactional = false) => new StubScope();
    }

    private class StubScope : IExecutionScope
    {
        public IDbConnection Connection => new StubConnection();
        public void Dispose() { }
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
        public Task CommitAsync(CancellationToken ct = default) => Task.CompletedTask;
        public Task RollbackAsync(CancellationToken ct = default) => Task.CompletedTask;
    }
}
