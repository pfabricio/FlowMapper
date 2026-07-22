using FlowMapper.Abstractions;

namespace FlowMapper.Providers.Abstractions;

public class ProviderFactory : IProviderFactory
{
    private readonly IProviderRegistry _registry;

    public ProviderFactory(IProviderRegistry registry)
    {
        _registry = registry;
    }

    public IDatabaseProvider CreateProvider(string providerName, string connectionString)
    {
        if (_registry.TryGetProvider(providerName, out var existing))
            return existing;

        throw new InvalidOperationException(
            $"Provider '{providerName}' is not registered. " +
            "Register providers via IProviderRegistry.RegisterProvider() before using ProviderFactory.");
    }
}
