using FlowMapper.Abstractions;
using FlowMapper.Providers.SqlServer;
using Xunit;

namespace FlowMapper.UnitTests.Providers;

public class SqlServerDialectFtsTests
{
    private readonly IDialect _dialect = new SqlServerDialect();

    [Fact]
    public void BuildFreeTextCondition_WithSingleColumn()
    {
        var result = _dialect.BuildFreeTextCondition(["Nome"], "@term");
        Assert.Equal("FREETEXT((Nome), @term)", result);
    }

    [Fact]
    public void BuildFreeTextCondition_WithMultipleColumns()
    {
        var result = _dialect.BuildFreeTextCondition(["Nome", "Descricao"], "@term");
        Assert.Equal("FREETEXT((Nome, Descricao), @term)", result);
    }

    [Fact]
    public void BuildContainsCondition_WithSingleColumn()
    {
        var result = _dialect.BuildContainsCondition(["Nome"], "@term");
        Assert.Equal("CONTAINS((Nome), @term)", result);
    }

    [Fact]
    public void BuildContainsCondition_WithMultipleColumns()
    {
        var result = _dialect.BuildContainsCondition(["Nome", "Descricao"], "@term");
        Assert.Equal("CONTAINS((Nome, Descricao), @term)", result);
    }

    [Fact]
    public void BuildRankOrderBy_ReturnsRankExpression()
    {
        var result = _dialect.BuildRankOrderBy(["Nome"], "@term");
        Assert.Contains("RANK DESC", result);
    }

    [Fact]
    public void FtsRequiresIndex_IsTrue()
    {
        Assert.True(_dialect.FtsRequiresIndex);
    }

    [Fact]
    public void FtsSupportsLanguage_IsFalse()
    {
        Assert.False(_dialect.FtsSupportsLanguage);
    }

    [Fact]
    public void FtsIndexErrorMessage_IsNotNull()
    {
        Assert.NotNull(_dialect.FtsIndexErrorMessage);
    }

    [Fact]
    public void VerifyFtsIndexSql_ContainsTableAndColumn()
    {
        var sql = _dialect.VerifyFtsIndexSql("Produtos", "Nome");
        Assert.NotNull(sql);
        Assert.Contains("Produtos", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Nome", sql, StringComparison.OrdinalIgnoreCase);
    }
}
