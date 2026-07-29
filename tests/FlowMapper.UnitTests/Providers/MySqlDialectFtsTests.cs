using FlowMapper.Abstractions;
using FlowMapper.Providers.MySql;
using Xunit;

namespace FlowMapper.UnitTests.Providers;

public class MySqlDialectFtsTests
{
    private readonly IDialect _dialect = new MySqlDialect();

    [Fact]
    public void BuildFreeTextCondition_WithSingleColumn()
    {
        var result = _dialect.BuildFreeTextCondition(["Nome"], "@term");
        Assert.Equal("MATCH(Nome) AGAINST (@term IN NATURAL LANGUAGE MODE)", result);
    }

    [Fact]
    public void BuildFreeTextCondition_WithMultipleColumns()
    {
        var result = _dialect.BuildFreeTextCondition(["Nome", "Descricao"], "@term");
        Assert.Equal("MATCH(Nome, Descricao) AGAINST (@term IN NATURAL LANGUAGE MODE)", result);
    }

    [Fact]
    public void BuildContainsCondition_WithBooleanMode()
    {
        var result = _dialect.BuildContainsCondition(["Nome"], "@term");
        Assert.Equal("MATCH(Nome) AGAINST (@term IN BOOLEAN MODE)", result);
    }

    [Fact]
    public void BuildRankOrderBy_ReturnsMatchExpression()
    {
        var result = _dialect.BuildRankOrderBy(["Nome"], "@term");
        Assert.Equal("MATCH(Nome) AGAINST (@term) DESC", result);
    }

    [Fact]
    public void FtsRequiresIndex_IsFalse()
    {
        Assert.False(_dialect.FtsRequiresIndex);
    }

    [Fact]
    public void FtsSupportsLanguage_IsFalse()
    {
        Assert.False(_dialect.FtsSupportsLanguage);
    }

    [Fact]
    public void FtsIndexErrorMessage_IsNull()
    {
        Assert.Null(_dialect.FtsIndexErrorMessage);
    }

    [Fact]
    public void VerifyFtsIndexSql_ContainsTableAndColumn()
    {
        var sql = _dialect.VerifyFtsIndexSql("Produtos", "Nome");
        Assert.NotNull(sql);
        Assert.Contains("Produtos", sql);
        Assert.Contains("Nome", sql);
    }
}
