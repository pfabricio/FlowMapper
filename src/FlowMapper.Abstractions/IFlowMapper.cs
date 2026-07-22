namespace FlowMapper.Abstractions;

public interface IFlowMapper
{
    TDestination Map<TSource, TDestination>(TSource source);
    IMapper<TSource, TDestination> GetMapper<TSource, TDestination>();
}
