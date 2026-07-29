using FlowMapper.Abstractions;
using FlowMapper.Providers.Oracle;
using Xunit;

namespace FlowMapper.UnitTests.Providers;

public class OracleDialectFtsTests
{
    private readonly IDialect _dialect = new OracleDialect();

    [Fact]
    public void BuildFreeTextCondition_WithSingleColumn()
    {
        var result = _dialect.BuildFreeTextCondition(["Nome"], ":term");
        Assert.Equal("CONTAINS(Nome, :term, 1) > 0", result);
    }

    [Fact]
    public void BuildFreeTextCondition_WithMultipleColumns()
    {
        var result = _dialect.BuildFreeTextCondition(["Nome", "Descricao"], ":term");
        Assert.Equal("CONTAINS(Nome, Descricao, :term, 1) > 0", result);
    }

    [Fact]
    public void BuildContainsCondition_WithMultipleColumns()
    {
        var result = _dialect.BuildContainsCondition(["Nome", "Descricao"], ":term");
        Assert.Equal("CONTAINS(Nome, Descricao, :term) > 0", result);
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
}
