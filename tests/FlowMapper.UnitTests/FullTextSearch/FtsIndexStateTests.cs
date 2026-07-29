using FlowMapper.FullTextSearch;
using Xunit;

namespace FlowMapper.UnitTests.FullTextSearch;

public class FtsIndexStateTests
{
    [Fact]
    public void DefaultState_IsConfigured()
    {
        var registry = new FullTextIndexRegistry();
        registry.Register<Produto>("Nome");

        var state = registry.GetState("Produto", "Nome");

        Assert.Equal(FtsIndexState.Configured, state);
    }

    [Fact]
    public void SetState_UpdatesState()
    {
        var registry = new FullTextIndexRegistry();
        registry.Register<Produto>("Nome");
        registry.SetState("Produto", "Nome", FtsIndexState.Verified);

        var state = registry.GetState("Produto", "Nome");

        Assert.Equal(FtsIndexState.Verified, state);
    }

    [Fact]
    public void UnregisteredColumn_ReturnsUnknown()
    {
        var registry = new FullTextIndexRegistry();

        var state = registry.GetState("Produto", "Inexistente");

        Assert.Equal(FtsIndexState.Unknown, state);
    }
}

public class Produto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
}
