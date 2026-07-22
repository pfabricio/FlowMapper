namespace FlowMapper.Abstractions;

public class ExecutionMetrics
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public TimeSpan TotalDuration => EndTime - StartTime;
    public TimeSpan DatabaseDuration { get; set; }
    public TimeSpan MappingDuration { get; set; }
    public int RowCount { get; set; }
}
