using FlowMapper.FullTextSearch;
using Xunit;

namespace FlowMapper.UnitTests.FullTextSearch;

public class FtsProfileDefinitionTests
{
    [Fact]
    public void Profile_RegistersColumns()
    {
        var profile = new ProdutoFtsProfile();
        var columns = profile.Registry.GetConfiguredColumns<Produto>();

        Assert.Contains("Nome", columns);
        Assert.Contains("Descricao", columns);
    }

    [Fact]
    public void Profile_IsConfigured_ReturnsTrueForRegisteredColumns()
    {
        var profile = new ProdutoFtsProfile();

        Assert.True(profile.Registry.IsConfigured<Produto>(x => x.Nome));
        Assert.True(profile.Registry.IsConfigured<Produto>(x => x.Descricao));
    }

    [Fact]
    public void Profile_IsConfigured_ReturnsFalseForUnregisteredColumns()
    {
        var profile = new ProdutoFtsProfile();

        Assert.False(profile.Registry.IsConfigured<Produto>(x => x.Id));
    }

    [Fact]
    public void Profile_DefaultState_IsConfigured()
    {
        var profile = new ProdutoFtsProfile();

        var state = profile.Registry.GetState("Produto", "Nome");

        Assert.Equal(FtsIndexState.Configured, state);
    }
}

public class ProdutoFtsProfile : FtsProfileDefinition
{
    public ProdutoFtsProfile()
    {
        Entity<Produto>()
            .HasFullTextIndex(x => x.Nome)
            .HasFullTextIndex(x => x.Descricao);
    }
}
