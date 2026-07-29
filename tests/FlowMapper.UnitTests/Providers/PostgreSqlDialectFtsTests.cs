using FlowMapper.Abstractions;
using FlowMapper.Providers.PostgreSql;
using Xunit;

namespace FlowMapper.UnitTests.Providers;

public class PostgreSqlDialectFtsTests
{
    [Fact]
    public void BuildFreeTextCondition_WithSingleColumn_DefaultLanguage()
    {
        var dialect = new PostgreSqlDialect();
        var result = dialect.BuildFreeTextCondition(["Nome"], "@term");
        Assert.Equal("to_tsvector('english', Nome) @@ plainto_tsquery('english', @term)", result);
    }

    [Fact]
    public void BuildFreeTextCondition_WithCustomLanguage()
    {
        var dialect = new PostgreSqlDialect("portuguese");
        var result = dialect.BuildFreeTextCondition(["Nome"], "@term");
        Assert.Equal("to_tsvector('portuguese', Nome) @@ plainto_tsquery('portuguese', @term)", result);
    }

    [Fact]
    public void BuildFreeTextCondition_WithMultipleColumns()
    {
        var dialect = new PostgreSqlDialect("portuguese");
        var result = dialect.BuildFreeTextCondition(["Nome", "Descricao"], "@term");
        Assert.Equal("to_tsvector('portuguese', Nome || ' ' || Descricao) @@ plainto_tsquery('portuguese', @term)", result);
    }

    [Fact]
    public void BuildContainsCondition_WithCustomLanguage()
    {
        var dialect = new PostgreSqlDialect("portuguese");
        var result = dialect.BuildContainsCondition(["Nome"], "@term");
        Assert.Equal("to_tsvector('portuguese', Nome) @@ to_tsquery('portuguese', @term)", result);
    }

    [Fact]
    public void BuildRankOrderBy_WithCustomLanguage()
    {
        var dialect = new PostgreSqlDialect("portuguese");
        var result = dialect.BuildRankOrderBy(["Nome"], "@term");
        Assert.Equal("ts_rank(to_tsvector('portuguese', Nome), plainto_tsquery('portuguese', @term)) DESC", result);
    }

    [Fact]
    public void FtsRequiresIndex_IsFalse()
    {
        var dialect = new PostgreSqlDialect();
        Assert.False(dialect.FtsRequiresIndex);
    }

    [Fact]
    public void FtsSupportsLanguage_IsTrue()
    {
        var dialect = new PostgreSqlDialect();
        Assert.True(dialect.FtsSupportsLanguage);
    }

    [Fact]
    public void FtsIndexErrorMessage_IsNull()
    {
        var dialect = new PostgreSqlDialect();
        Assert.Null(dialect.FtsIndexErrorMessage);
    }

    [Fact]
    public void VerifyFtsIndexSql_IsNotNull()
    {
        var dialect = new PostgreSqlDialect();
        var sql = dialect.VerifyFtsIndexSql("Produtos", "Nome");
        Assert.NotNull(sql);
        Assert.Contains("pg_indexes", sql);
    }
}
