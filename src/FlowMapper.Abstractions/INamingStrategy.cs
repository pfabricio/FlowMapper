namespace FlowMapper.Abstractions;

public interface INamingStrategy
{
    string Apply(string columnName);
}
