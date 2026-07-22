namespace FlowMapper.Core;

public class DataReaderMappingExpression<TDestination>
{
    internal List<ExplicitMapping> ExplicitMappings { get; } = new();
}
