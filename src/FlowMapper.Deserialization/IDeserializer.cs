using FlowMapper.Abstractions;

namespace FlowMapper.Deserialization;

public interface IDeserializer
{
    T FromJson<T>(string json);
    List<T> FromJsonList<T>(string json);
    T FromXml<T>(string xml);
    List<T> FromText<T>(string[] lines, TextDelimiter delimiter, bool hasHeader = true);
}
