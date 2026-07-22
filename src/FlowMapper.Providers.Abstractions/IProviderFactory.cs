using System.Data;

using FlowMapper.Abstractions;

namespace FlowMapper.Providers.Abstractions;

public interface IProviderFactory
{
    IDatabaseProvider CreateProvider(string providerName, string connectionString);
}
