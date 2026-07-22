using System.Collections.Concurrent;
using FlowMapper.Abstractions;

namespace FlowMapper.Providers.Abstractions;

public class ProviderRegistry : IProviderRegistry
{
    private readonly ConcurrentDictionary<string, IDatabaseProvider> _providers = new(StringComparer.OrdinalIgnoreCase);

    public IDatabaseProvider GetProvider(string name)
    {
        if (_providers.TryGetValue(name, out var provider))
            return provider;

        throw new InvalidOperationException(
            $"Database provider '{name}' is not registered. " +
            $"Registered providers: {string.Join(", ", _providers.Keys)}");
    }

    public bool TryGetProvider(string name, out IDatabaseProvider? provider)
    {
        return _providers.TryGetValue(name, out provider);
    }

    public void RegisterProvider(string name, IDatabaseProvider provider)
    {
        if (!_providers.TryAdd(name, provider))
            throw new InvalidOperationException(
                $"Database provider '{name}' is already registered.");
    }

    public IReadOnlyCollection<string> RegisteredProviders => _providers.Keys.ToList();
}
