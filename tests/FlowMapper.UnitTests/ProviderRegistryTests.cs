using FlowMapper.Abstractions;
using FlowMapper.Providers.Abstractions;
using Xunit;

namespace FlowMapper.UnitTests;

public class ProviderRegistryTests
{
    private sealed class FakeProvider : IDatabaseProvider
    {
        public string Name => "Fake";
        public IDialect Dialect => throw new NotImplementedException();
        public Version Version => new(1, 0);
        public System.Data.IDbConnection CreateConnection() => throw new NotImplementedException();
        public System.Data.IDbCommand CreateCommand(string sql, System.Data.IDbConnection connection, System.Data.IDbTransaction? transaction = null) => throw new NotImplementedException();
        public System.Data.IDataParameter CreateParameter(string name, object? value) => throw new NotImplementedException();
    }

    [Fact]
    public void RegisterProvider_ThenGetProvider_ReturnsSameInstance()
    {
        var registry = new ProviderRegistry();
        var provider = new FakeProvider();

        registry.RegisterProvider("Test", provider);
        var retrieved = registry.GetProvider("Test");

        Assert.Same(provider, retrieved);
    }

    [Fact]
    public void RegisterProvider_DuplicateName_Throws()
    {
        var registry = new ProviderRegistry();
        registry.RegisterProvider("Test", new FakeProvider());

        Assert.Throws<InvalidOperationException>(() =>
            registry.RegisterProvider("Test", new FakeProvider()));
    }

    [Fact]
    public void GetProvider_UnknownName_Throws()
    {
        var registry = new ProviderRegistry();

        Assert.Throws<InvalidOperationException>(() =>
            registry.GetProvider("NonExistent"));
    }

    [Fact]
    public void TryGetProvider_Registered_ReturnsTrue()
    {
        var registry = new ProviderRegistry();
        registry.RegisterProvider("Test", new FakeProvider());

        var found = registry.TryGetProvider("Test", out var provider);

        Assert.True(found);
        Assert.NotNull(provider);
    }

    [Fact]
    public void TryGetProvider_NotRegistered_ReturnsFalse()
    {
        var registry = new ProviderRegistry();

        var found = registry.TryGetProvider("NonExistent", out var provider);

        Assert.False(found);
        Assert.Null(provider);
    }

    [Fact]
    public void RegisteredProviders_ReturnsAllKeys()
    {
        var registry = new ProviderRegistry();
        registry.RegisterProvider("A", new FakeProvider());
        registry.RegisterProvider("B", new FakeProvider());

        var names = registry.RegisteredProviders;

        Assert.Contains("A", names);
        Assert.Contains("B", names);
        Assert.Equal(2, names.Count);
    }

    [Fact]
    public void RegisterProvider_CaseInsensitive()
    {
        var registry = new ProviderRegistry();
        registry.RegisterProvider("SqlServer", new FakeProvider());

        Assert.Throws<InvalidOperationException>(() =>
            registry.RegisterProvider("sqlserver", new FakeProvider()));
    }
}
