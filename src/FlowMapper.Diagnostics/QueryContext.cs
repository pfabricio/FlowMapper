using FlowMapper.Abstractions;

namespace FlowMapper.Diagnostics;

public class QueryContext
{
    public string Sql { get; init; } = string.Empty;
    public object? Parameters { get; init; }
    public IDatabaseProvider? Provider { get; init; }
}
