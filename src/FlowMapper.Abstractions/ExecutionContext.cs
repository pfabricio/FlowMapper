namespace FlowMapper.Abstractions;

public class ExecutionContext<T>
{
    public string Sql { get; }
    public object? Parameters { get; }
    public ExecutionOptions? Options { get; }
    public ExecutionPhase Phase { get; set; }
    public ExecutionMetrics Metrics { get; } = new();
    public T? Result { get; set; }
    public Exception? Exception { get; set; }

    public ExecutionContext(string sql, object? parameters, ExecutionOptions? options)
    {
        Sql = sql;
        Parameters = parameters;
        Options = options;
    }
}
