using System.Data;
using FlowMapper.Abstractions;
using FlowMapper.Diagnostics;
using FlowMapper.Diagnostics.Rules;
using FlowMapper.FullTextSearch;
using Xunit;

namespace FlowMapper.UnitTests.Diagnostics;

public class FullTextIndexRuleTests
{
    [Fact]
    public void Analyze_ConfiguredTableAndColumn_EmitsInfo()
    {
        var registry = new FullTextIndexRegistry();
        registry.Register<Product>("Name");
        var rule = new FullTextIndexRule(registry);

        var ctx = new QueryContext
        {
            Sql = "SELECT * FROM Product WHERE Id = 1",
            Provider = new SpyProvider("SqlServer")
        };

        var result = rule.Analyze(ctx).ToList();

        Assert.Single(result);
        Assert.Equal("FM1001", result[0].Code);
        Assert.Equal(DiagnosticSeverity.Info, result[0].Severity);
        Assert.Equal("Product", result[0].Table);
        Assert.Equal("Name", result[0].Column);
    }

    [Fact]
    public void Analyze_TableNotInSql_DoesNotEmit()
    {
        var registry = new FullTextIndexRegistry();
        registry.Register<Product>("Name");
        var rule = new FullTextIndexRule(registry);

        var ctx = new QueryContext
        {
            Sql = "SELECT * FROM Categories WHERE Id = 1",
            Provider = new SpyProvider("SqlServer")
        };

        Assert.Empty(rule.Analyze(ctx));
    }

    [Fact]
    public void Analyze_NoProvider_DoesNotEmit()
    {
        var registry = new FullTextIndexRegistry();
        registry.Register<Product>("Name");
        var rule = new FullTextIndexRule(registry);

        var ctx = new QueryContext
        {
            Sql = "SELECT * FROM Product WHERE Id = 1",
            Provider = null
        };

        Assert.Empty(rule.Analyze(ctx));
    }

    [Fact]
    public void Analyze_NoConfiguredColumns_DoesNotEmit()
    {
        var registry = new FullTextIndexRegistry();
        var rule = new FullTextIndexRule(registry);

        var ctx = new QueryContext
        {
            Sql = "SELECT * FROM Product WHERE Id = 1",
            Provider = new SpyProvider("SqlServer")
        };

        Assert.Empty(rule.Analyze(ctx));
    }

    [Fact]
    public void CanAnalyze_AlwaysTrue()
    {
        var registry = new FullTextIndexRegistry();
        var rule = new FullTextIndexRule(registry);
        Assert.True(rule.CanAnalyze(new QueryContext { Sql = "" }));
    }

    [Fact]
    public void Analyze_WithSchemaInspectionVerified_EmitsInfo()
    {
        var registry = new FullTextIndexRegistry();
        registry.Register<Product>("Name");
        var inspector = new StubSchemaInspector(FtsIndexState.Verified);
        var rule = new FullTextIndexRule(registry, inspector, new FlowMapperDiagnosticsOptions
        {
            EnableSchemaInspection = true
        });

        var ctx = new QueryContext
        {
            Sql = "SELECT * FROM Product WHERE Id = 1",
            Provider = new SpyProvider("SqlServer")
        };

        var result = rule.Analyze(ctx).ToList();

        Assert.Single(result);
        Assert.Equal("FM1001", result[0].Code);
        Assert.Equal(DiagnosticSeverity.Info, result[0].Severity);
        Assert.Contains("verified", result[0].Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Analyze_WithSchemaInspectionMissing_EmitsWarning()
    {
        var registry = new FullTextIndexRegistry();
        registry.Register<Product>("Name");
        var inspector = new StubSchemaInspector(FtsIndexState.Missing);
        var rule = new FullTextIndexRule(registry, inspector, new FlowMapperDiagnosticsOptions
        {
            EnableSchemaInspection = true
        });

        var ctx = new QueryContext
        {
            Sql = "SELECT * FROM Product WHERE Id = 1",
            Provider = new SpyProvider("SqlServer")
        };

        var result = rule.Analyze(ctx).ToList();

        Assert.Single(result);
        Assert.Equal("FM1001", result[0].Code);
        Assert.Equal(DiagnosticSeverity.Warning, result[0].Severity);
        Assert.Contains("missing", result[0].Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Analyze_WithSchemaInspectionUnknown_FallsBackToRegistry()
    {
        var registry = new FullTextIndexRegistry();
        registry.Register<Product>("Name");
        var inspector = new StubSchemaInspector(FtsIndexState.Unknown);
        var rule = new FullTextIndexRule(registry, inspector, new FlowMapperDiagnosticsOptions
        {
            EnableSchemaInspection = true
        });

        var ctx = new QueryContext
        {
            Sql = "SELECT * FROM Product WHERE Id = 1",
            Provider = new SpyProvider("SqlServer")
        };

        var result = rule.Analyze(ctx).ToList();

        Assert.Empty(result);
    }

    [Fact]
    public void Analyze_SchemaInspectionDisabled_UsesRegistryState()
    {
        var registry = new FullTextIndexRegistry();
        registry.Register<Product>("Name");
        var inspector = new StubSchemaInspector(FtsIndexState.Verified);
        var rule = new FullTextIndexRule(registry, inspector, new FlowMapperDiagnosticsOptions
        {
            EnableSchemaInspection = false
        });

        var ctx = new QueryContext
        {
            Sql = "SELECT * FROM Product WHERE Id = 1",
            Provider = new SpyProvider("SqlServer")
        };

        var result = rule.Analyze(ctx).ToList();

        Assert.Single(result);
        Assert.Equal(DiagnosticSeverity.Info, result[0].Severity);
        Assert.Contains("via profile", result[0].Message, StringComparison.OrdinalIgnoreCase);
    }

    private class Product { }

    private class StubSchemaInspector : ISchemaInspector
    {
        private readonly FtsIndexState _state;
        public StubSchemaInspector(FtsIndexState state) => _state = state;
        public FtsIndexState VerifyIndex(string table, string column, IDatabaseProvider provider) => _state;
        public void ClearCache() { }
    }

    private class SpyProvider(string name) : IDatabaseProvider
    {
        public string Name => name;
        public Version Version => new(1, 0);
        public IDialect Dialect => new SpyDialect();
        public IDbConnection CreateConnection() => throw new NotImplementedException();
        public IDbCommand CreateCommand(string sql, IDbConnection connection, IDbTransaction? transaction) => throw new NotImplementedException();
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
        public string? VerifyFtsIndexSql(string table, string column) => "";
    }
}
