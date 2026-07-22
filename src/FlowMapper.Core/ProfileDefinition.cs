namespace FlowMapper.Core;

public abstract class ProfileDefinition
{
    public string ProfileName { get; protected set; } = "Default";
    public MappingPolicy Policy { get; protected set; } = new();
    public List<MappingRegistration> Registrations { get; } = new();

    protected MappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>()
    {
        var expression = new MappingExpression<TSource, TDestination>(ProfileName, Policy);
        Registrations.Add(new MappingRegistration
        {
            SourceType = typeof(TSource),
            DestinationType = typeof(TDestination),
            Expression = expression
        });
        return expression;
    }

    protected DataReaderMapping<TDestination> CreateDataReaderMap<TDestination>()
    {
        var mapping = new DataReaderMapping<TDestination>();
        Registrations.Add(new MappingRegistration
        {
            SourceType = typeof(DataReaderMapping<>).MakeGenericType(typeof(TDestination)),
            DestinationType = typeof(TDestination),
            DataReaderMapping = mapping
        });
        return mapping;
    }
}

public class MappingRegistration
{
    public Type SourceType { get; init; } = null!;
    public Type DestinationType { get; init; } = null!;
    public object? Expression { get; init; }
    public object? DataReaderMapping { get; init; }
}

public class DataReaderMapping<TDestination>
{
    internal List<DataReaderColumnBinding> ColumnBindings { get; } = new();

    public DataReaderMapping<TDestination> BindColumn<TProperty>(string columnName, Func<TDestination, TProperty> propertySelector)
    {
        ColumnBindings.Add(new DataReaderColumnBinding
        {
            ColumnName = columnName,
            PropertyName = string.Empty
        });
        return this;
    }
}

public class DataReaderColumnBinding
{
    public string ColumnName { get; init; } = string.Empty;
    public string PropertyName { get; init; } = string.Empty;
}
