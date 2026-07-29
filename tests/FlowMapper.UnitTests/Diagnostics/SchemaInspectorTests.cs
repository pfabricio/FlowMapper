using System.Data;
using FlowMapper.Abstractions;
using FlowMapper.Diagnostics;
using FlowMapper.FullTextSearch;
using Xunit;

namespace FlowMapper.UnitTests.Diagnostics;

public class SchemaInspectorTests
{
    [Fact]
    public void VerifyIndex_SqlNull_ReturnsUnknown()
    {
        var inspector = new SchemaInspector();
        var provider = new NullVerifySqlProvider();

        var result = inspector.VerifyIndex("Product", "Name", provider);

        Assert.Equal(FtsIndexState.Unknown, result);
    }

    [Fact]
    public void VerifyIndex_ResultNotNull_ReturnsVerified()
    {
        var inspector = new SchemaInspector();
        var provider = new FakeDbProvider(returnValue: 1);

        var result = inspector.VerifyIndex("Product", "Name", provider);

        Assert.Equal(FtsIndexState.Verified, result);
    }

    [Fact]
    public void VerifyIndex_ResultNull_ReturnsMissing()
    {
        var inspector = new SchemaInspector();
        var provider = new FakeDbProvider(returnValue: null);

        var result = inspector.VerifyIndex("Product", "Name", provider);

        Assert.Equal(FtsIndexState.Missing, result);
    }

    [Fact]
    public void VerifyIndex_ExceptionDuringQuery_ReturnsUnknown()
    {
        var inspector = new SchemaInspector();
        var provider = new BrokenDbProvider();

        var result = inspector.VerifyIndex("Product", "Name", provider);

        Assert.Equal(FtsIndexState.Unknown, result);
    }

    [Fact]
    public void VerifyIndex_CachesResult()
    {
        var inspector = new SchemaInspector();
        var provider = new FakeDbProvider(returnValue: 1);
        var provider2 = new FakeDbProvider(returnValue: null);

        var first = inspector.VerifyIndex("Product", "Name", provider);
        var second = inspector.VerifyIndex("Product", "Name", provider2);

        Assert.Equal(FtsIndexState.Verified, first);
        Assert.Equal(FtsIndexState.Verified, second);
    }

    [Fact]
    public void VerifyIndex_DifferentProvider_DoesNotShareCache()
    {
        var inspector = new SchemaInspector();

        var result1 = inspector.VerifyIndex("Product", "Name",
            new FakeDbProvider(returnValue: 1, name: "SqlServer"));
        var result2 = inspector.VerifyIndex("Product", "Name",
            new FakeDbProvider(returnValue: null, name: "PostgreSql"));

        Assert.Equal(FtsIndexState.Verified, result1);
        Assert.Equal(FtsIndexState.Missing, result2);
    }

    [Fact]
    public void ClearCache_RemovesAllEntries()
    {
        var inspector = new SchemaInspector();
        var provider = new FakeDbProvider(returnValue: 1);

        inspector.VerifyIndex("Product", "Name", provider);
        inspector.ClearCache();

        var provider2 = new FakeDbProvider(returnValue: null, name: "SqlServer");
        var result = inspector.VerifyIndex("Product", "Name", provider2);

        Assert.Equal(FtsIndexState.Missing, result);
    }

    private class NullVerifySqlProvider : IDatabaseProvider
    {
        public string Name => "Test";
        public Version Version => new(1, 0);
        public IDialect Dialect => new NullVerifyDialect();
        public IDbConnection CreateConnection() => throw new NotImplementedException();
        public IDbCommand CreateCommand(string sql, IDbConnection connection, IDbTransaction? transaction) => throw new NotImplementedException();
        public IDataParameter CreateParameter(string name, object? value) => throw new NotImplementedException();
    }

    private class NullVerifyDialect : IDialect
    {
        public bool FtsRequiresIndex => false;
        public bool FtsSupportsLanguage => false;
        public string? FtsIndexErrorMessage => null;
        public string ApplyPagination(string sql, int offset, int limit) => sql;
        public string GetIdentityQuery() => "";
        public string NormalizeParameter(string name) => name;
        public string BuildFreeTextCondition(IReadOnlyList<string> columns, string parameterName) => "";
        public string BuildContainsCondition(IReadOnlyList<string> columns, string parameterName) => "";
        public string BuildRankOrderBy(IReadOnlyList<string> columns, string parameterName) => null!;
        public string? VerifyFtsIndexSql(string table, string column) => null;
    }

    private class FakeDbProvider : IDatabaseProvider
    {
        private readonly object? _returnValue;
        public string Name { get; }
        public Version Version => new(1, 0);
        public IDialect Dialect => new SpyDialect();

        public FakeDbProvider(object? returnValue, string name = "Test")
        {
            _returnValue = returnValue;
            Name = name;
        }

        public IDbConnection CreateConnection() => new FakeConnection(_returnValue);
        public IDbCommand CreateCommand(string sql, IDbConnection connection, IDbTransaction? transaction)
            => new FakeCommand(_returnValue);
        public IDataParameter CreateParameter(string name, object? value) => throw new NotImplementedException();
    }

    private class SpyDialect : IDialect
    {
        public bool FtsRequiresIndex => true;
        public bool FtsSupportsLanguage => false;
        public string? FtsIndexErrorMessage => "FTS index required";
        public string ApplyPagination(string sql, int offset, int limit) => sql;
        public string GetIdentityQuery() => "";
        public string NormalizeParameter(string name) => name;
        public string BuildFreeTextCondition(IReadOnlyList<string> columns, string parameterName) => "";
        public string BuildContainsCondition(IReadOnlyList<string> columns, string parameterName) => "";
        public string BuildRankOrderBy(IReadOnlyList<string> columns, string parameterName) => null!;
        public string? VerifyFtsIndexSql(string table, string column) => "SELECT 1 FROM test";
    }

    private class FakeConnection : IDbConnection
    {
        private readonly object? _returnValue;
        public FakeConnection(object? returnValue) => _returnValue = returnValue;
        public string? ConnectionString { get => ""; set => _ = value; }
        public int ConnectionTimeout => 30;
        public string Database => "test";
        public ConnectionState State => ConnectionState.Open;
        public IDbTransaction BeginTransaction() => throw new NotImplementedException();
        public IDbTransaction BeginTransaction(IsolationLevel il) => throw new NotImplementedException();
        public void ChangeDatabase(string databaseName) { }
        public void Close() { }
        public IDbCommand CreateCommand() => new FakeCommand(_returnValue);
        public void Dispose() { }
        public void Open() { }
    }

    private class FakeCommand : IDbCommand
    {
        private readonly object? _returnValue;
        public FakeCommand(object? returnValue) => _returnValue = returnValue;
        public string? CommandText { get => ""; set => _ = value; }
        public IDbConnection? Connection { get => null; set => _ = value; }
        public int CommandTimeout { get => 0; set => _ = value; }
        public CommandType CommandType { get => CommandType.Text; set => _ = value; }
        public IDataParameterCollection Parameters => throw new NotImplementedException();
        public IDbTransaction? Transaction { get => null; set => _ = value; }
        public UpdateRowSource UpdatedRowSource { get => UpdateRowSource.None; set => _ = value; }
        public void Cancel() { }
        public IDbDataParameter CreateParameter() => throw new NotImplementedException();
        public void Dispose() { }
        public int ExecuteNonQuery() => throw new NotImplementedException();
        public IDataReader ExecuteReader() => throw new NotImplementedException();
        public IDataReader ExecuteReader(CommandBehavior behavior) => throw new NotImplementedException();
        public object? ExecuteScalar() => _returnValue;
        public void Prepare() { }
    }

    private class BrokenDbProvider : IDatabaseProvider
    {
        public string Name => "Broken";
        public Version Version => new(1, 0);
        public IDialect Dialect => new BrokenDialect();
        public IDbConnection CreateConnection() => throw new InvalidOperationException("Connection failed");
        public IDbCommand CreateCommand(string sql, IDbConnection connection, IDbTransaction? transaction) => throw new NotImplementedException();
        public IDataParameter CreateParameter(string name, object? value) => throw new NotImplementedException();
    }

    private class BrokenDialect : IDialect
    {
        public bool FtsRequiresIndex => true;
        public bool FtsSupportsLanguage => false;
        public string? FtsIndexErrorMessage => null;
        public string ApplyPagination(string sql, int offset, int limit) => sql;
        public string GetIdentityQuery() => "";
        public string NormalizeParameter(string name) => name;
        public string BuildFreeTextCondition(IReadOnlyList<string> columns, string parameterName) => "";
        public string BuildContainsCondition(IReadOnlyList<string> columns, string parameterName) => "";
        public string BuildRankOrderBy(IReadOnlyList<string> columns, string parameterName) => null!;
        public string? VerifyFtsIndexSql(string table, string column) => "SELECT 1 FROM test";
    }
}
