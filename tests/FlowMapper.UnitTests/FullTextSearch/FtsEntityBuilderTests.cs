using FlowMapper.FullTextSearch;
using Xunit;

namespace FlowMapper.UnitTests.FullTextSearch;

public class FtsEntityBuilderTests
{
    [Fact]
    public void HasFullTextIndex_WithMultipleColumns_RegistersAll()
    {
        var registry = new FullTextIndexRegistry();

        new FtsEntityBuilder<Produto>(registry)
            .HasFullTextIndex(x => x.Nome)
            .HasFullTextIndex(x => x.Descricao);

        var columns = registry.GetConfiguredColumns<Produto>();

        Assert.Contains("Nome", columns);
        Assert.Contains("Descricao", columns);
        Assert.Equal(2, columns.Count);
    }

    [Fact]
    public void HasFullTextIndex_SameColumnTwice_DoesNotDuplicate()
    {
        var registry = new FullTextIndexRegistry();

        new FtsEntityBuilder<Produto>(registry)
            .HasFullTextIndex(x => x.Nome)
            .HasFullTextIndex(x => x.Nome);

        var columns = registry.GetConfiguredColumns<Produto>();

        Assert.Single(columns);
    }
}
