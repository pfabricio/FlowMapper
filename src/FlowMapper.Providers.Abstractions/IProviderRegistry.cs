using FlowMapper.Abstractions;

namespace FlowMapper.Providers.Abstractions;

public interface IProviderRegistry
{
    IDatabaseProvider GetProvider(string name);
    bool TryGetProvider(string name, out IDatabaseProvider? provider);
    void RegisterProvider(string name, IDatabaseProvider provider);
    IReadOnlyCollection<string> RegisteredProviders { get; }
}
