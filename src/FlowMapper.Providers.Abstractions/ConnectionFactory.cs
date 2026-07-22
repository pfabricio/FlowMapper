using System.Data;
using FlowMapper.Abstractions;

namespace FlowMapper.Providers.Abstractions;

public class ConnectionFactory : IConnectionFactory
{
    private readonly Func<IDbConnection> _defaultFactory;
    private readonly Dictionary<string, Func<IDbConnection>> _factories;

    public ConnectionFactory(Func<IDbConnection> defaultFactory)
    {
        _defaultFactory = defaultFactory;
        _factories = new Dictionary<string, Func<IDbConnection>>();
    }

    public ConnectionFactory(
        Dictionary<string, Func<IDbConnection>> factories,
        string defaultName)
    {
        _factories = factories;
        _defaultFactory = factories.GetValueOrDefault(defaultName, () => throw new InvalidOperationException($"Connection '{defaultName}' not found"));
    }

    public IDbConnection CreateConnection(string? name = null)
    {
        if (name == null)
            return _defaultFactory();

        if (_factories.TryGetValue(name, out var factory))
            return factory();

        throw new InvalidOperationException($"Connection '{name}' not registered");
    }
}
