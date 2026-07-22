using System.Data;

namespace FlowMapper.Abstractions;

public interface IDataReaderMapper
{
    IEnumerable<T> Map<T>(IDataReader reader, MappingOptions options);

    T? MapSingle<T>(IDataReader reader, MappingOptions options);

    T? MapScalar<T>(object? value);
}
