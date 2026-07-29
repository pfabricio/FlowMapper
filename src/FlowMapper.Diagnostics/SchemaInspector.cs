using System.Collections.Concurrent;
using FlowMapper.Abstractions;
using FlowMapper.FullTextSearch;

namespace FlowMapper.Diagnostics;

public sealed class SchemaInspector : ISchemaInspector
{
    private readonly ConcurrentDictionary<(string Table, string Column, string Provider), FtsIndexState> _cache = new();

    public FtsIndexState VerifyIndex(string table, string column, IDatabaseProvider provider)
    {
        var key = (table, column, provider.Name);

        if (_cache.TryGetValue(key, out var state))
            return state;

        var sql = provider.Dialect.VerifyFtsIndexSql(table, column);

        if (sql is null)
        {
            _cache[key] = FtsIndexState.Unknown;
            return FtsIndexState.Unknown;
        }

        try
        {
            using var connection = provider.CreateConnection();
            connection.Open();
            using var command = provider.CreateCommand(sql, connection);
            var result = command.ExecuteScalar();

            state = result is not null ? FtsIndexState.Verified : FtsIndexState.Missing;
        }
        catch
        {
            state = FtsIndexState.Unknown;
        }

        _cache[key] = state;
        return state;
    }

    public void ClearCache() => _cache.Clear();
}
