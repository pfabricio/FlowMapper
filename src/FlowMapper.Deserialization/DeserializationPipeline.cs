using System.Text.Json;
using System.Xml.Linq;
using System.Reflection;
using FlowMapper.Abstractions;
using FlowMapper.Execution;
using FlowMapper.Materializer;

namespace FlowMapper.Deserialization;

public class DeserializationPipeline : IDeserializer
{
    public T FromJson<T>(string json)
    {
        var doc = JsonDocument.Parse(json);
        var dict = FlattenJson(doc.RootElement, "");
        return MaterializeFromFlat<T>(dict);
    }

    public List<T> FromJsonList<T>(string json)
    {
        var doc = JsonDocument.Parse(json);
        var list = new List<T>();
        foreach (var element in doc.RootElement.EnumerateArray())
        {
            var dict = FlattenJson(element, "");
            list.Add(MaterializeFromFlat<T>(dict));
        }
        return list;
    }

    public T FromXml<T>(string xml)
    {
        var doc = XDocument.Parse(xml);
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        WalkXml(doc.Root!, "", dict);
        return MaterializeFromFlat<T>(dict);
    }

    public List<T> FromText<T>(string[] lines, TextDelimiter delimiter, bool hasHeader = true)
    {
        var sep = delimiter == TextDelimiter.PontoVirgula ? ';' : '\t';
        var result = new List<T>();
        var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var start = hasHeader ? 1 : 0;

        Dictionary<int, string>? colMap = null;
        if (hasHeader && lines.Length > 0)
        {
            var headers = lines[0].Split(sep);
            colMap = new Dictionary<int, string>();
            for (int i = 0; i < headers.Length; i++)
                colMap[i] = headers[i].Trim();
        }

        for (int i = start; i < lines.Length; i++)
        {
            var parts = lines[i].Split(sep);
            var instance = Activator.CreateInstance<T>();
            for (int j = 0; j < parts.Length; j++)
            {
                var propName = colMap != null && colMap.ContainsKey(j)
                    ? colMap[j]
                    : (j < props.Length ? props[j].Name : null);
                if (propName == null) continue;

                var prop = Array.Find(props, p =>
                    p.Name.Equals(propName, StringComparison.OrdinalIgnoreCase));
                if (prop != null && prop.CanWrite)
                {
                    var value = Convert.ChangeType(parts[j].Trim(), prop.PropertyType);
                    prop.SetValue(instance, value);
                }
            }
            result.Add(instance);
        }
        return result;
    }

    private static Dictionary<string, string> FlattenJson(JsonElement element, string prefix)
    {
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var prop in element.EnumerateObject())
            {
                var key = string.IsNullOrEmpty(prefix) ? prop.Name : $"{prefix}:{prop.Name}";
                if (prop.Value.ValueKind == JsonValueKind.Object)
                {
                    foreach (var kv in FlattenJson(prop.Value, key))
                        dict[kv.Key] = kv.Value;
                }
                else if (prop.Value.ValueKind == JsonValueKind.Array)
                {
                    dict[key] = prop.Value.GetRawText();
                }
                else
                {
                    dict[key] = prop.Value.ToString();
                }
            }
        }
        return dict;
    }

    private static void WalkXml(XElement element, string prefix, Dictionary<string, string> dict)
    {
        foreach (var child in element.Elements())
        {
            var key = string.IsNullOrEmpty(prefix) ? child.Name.LocalName : $"{prefix}:{child.Name.LocalName}";
            if (child.HasElements)
                WalkXml(child, key, dict);
            else
                dict[key] = child.Value;
        }
    }

    private static T MaterializeFromFlat<T>(Dictionary<string, string> flat)
    {
        var plan = Materializer.Materializer.BuildPlanFlat<T>();
        var grouped = GroupBindings(plan);
        return BuildObject<T>(flat, grouped);
    }

    private static GroupedBindings GroupBindings(MaterializationPlan plan)
    {
        var grouped = new GroupedBindings();
        foreach (var binding in plan.Bindings)
        {
            var parts = binding.ColumnName.Split('_', 2);
            if (parts.Length == 2 && IsNestedType(parts[0], plan.TargetType))
            {
                if (!grouped.Nested.ContainsKey(parts[0]))
                    grouped.Nested[parts[0]] = new List<MaterializationBinding>();
                grouped.Nested[parts[0]].Add(new MaterializationBinding
                {
                    ColumnName = parts[1],
                    PropertyName = parts[1],
                    PropertyType = binding.PropertyType
                });
            }
            else
            {
                grouped.Flat.Add(binding);
            }
        }
        return grouped;
    }

    private static bool IsNestedType(string name, Type parentType)
    {
        var prop = parentType.GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        if (prop == null) return false;
        var t = prop.PropertyType;
        return t.IsClass && t != typeof(string) && !t.IsValueType
            && !typeof(System.Collections.IEnumerable).IsAssignableFrom(t);
    }

    private static T BuildObject<T>(Dictionary<string, string> flat, GroupedBindings grouped)
    {
        var instance = Activator.CreateInstance<T>();

        foreach (var binding in grouped.Flat)
        {
            if (flat.TryGetValue(binding.ColumnName, out var val))
            {
                var prop = typeof(T).GetProperty(binding.PropertyName,
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (prop != null && prop.CanWrite)
                {
                    var converted = Convert.ChangeType(val, prop.PropertyType);
                    prop.SetValue(instance, converted);
                }
            }
        }

        foreach (var kv in grouped.Nested)
        {
            var prop = typeof(T).GetProperty(kv.Key,
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (prop == null) continue;

            var nestedPlan = new MaterializationPlan
            {
                TargetType = prop.PropertyType,
                Bindings = kv.Value
            };
            var nestedGrouped = GroupBindings(nestedPlan);
            var nestedObj = BuildObjectByType(flat, prop.PropertyType, nestedGrouped);
            prop.SetValue(instance, nestedObj);
        }

        return instance;
    }

    private static object? BuildObjectByType(Dictionary<string, string> flat, Type type, GroupedBindings grouped)
    {
        var instance = Activator.CreateInstance(type);

        foreach (var binding in grouped.Flat)
        {
            var key = binding.ColumnName;
            if (flat.TryGetValue(key, out var val))
            {
                var prop = type.GetProperty(binding.PropertyName,
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (prop != null && prop.CanWrite)
                {
                    var converted = Convert.ChangeType(val, prop.PropertyType);
                    prop.SetValue(instance, converted);
                }
            }
        }

        foreach (var kv in grouped.Nested)
        {
            var prop = type.GetProperty(kv.Key,
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (prop == null) continue;

            var nestedPlan = new MaterializationPlan
            {
                TargetType = prop.PropertyType,
                Bindings = kv.Value
            };
            var nestedGrouped = GroupBindings(nestedPlan);
            var nestedObj = BuildObjectByType(flat, prop.PropertyType, nestedGrouped);
            prop.SetValue(instance, nestedObj);
        }

        return instance;
    }
}

public class GroupedBindings
{
    public List<MaterializationBinding> Flat { get; set; } = new();
    public Dictionary<string, List<MaterializationBinding>> Nested { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
