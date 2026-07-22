using System.Data;

namespace FlowMapper.Abstractions;

public class ExecutionOptions
{
    public int? Timeout { get; set; }
    public CommandType CommandType { get; set; } = CommandType.Text;
    public string? ConnectionName { get; set; }
    public string? CacheKey { get; set; }
    public TimeSpan? CacheExpiration { get; set; }
}
