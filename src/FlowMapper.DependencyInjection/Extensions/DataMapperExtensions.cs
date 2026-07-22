using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using FlowMapper.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace FlowMapper.DependencyInjection;

public static class DataMapperExtensions
{
    private static IFlowMapper? _mapper;

    public static void Initialize(IServiceProvider serviceProvider)
    {
        _mapper = serviceProvider.GetRequiredService<IFlowMapper>();
    }

    public static async Task<List<TDest>> Map<TSource, TDest>(
        this Task<IEnumerable<TSource>> task)
    {
        var source = await task;
        return MapEnumerable<TSource, TDest>(source);
    }

    public static async Task<List<TDest>> Map<TSource, TDest>(
        this Task<List<TSource>> task)
    {
        var source = await task;
        return MapEnumerable<TSource, TDest>(source);
    }

    public static async Task<TDest> Map<TSource, TDest>(
        this Task<TSource> task)
    {
        var source = await task;
        return GetMapper<TSource, TDest>().Map(source);
    }

    public static async IAsyncEnumerable<TDest> Map<TSource, TDest>(
        this IAsyncEnumerable<TSource> source)
    {
        var mapper = GetMapper<TSource, TDest>();
        await foreach (var item in source)
        {
            yield return mapper.Map(item);
        }
    }

    private static List<TDest> MapEnumerable<TSource, TDest>(IEnumerable<TSource> source)
    {
        var mapper = GetMapper<TSource, TDest>();
        return source.Select(s => mapper.Map(s)).ToList();
    }

    private static IMapper<TSource, TDest> GetMapper<TSource, TDest>()
    {
        if (_mapper == null)
            throw new InvalidOperationException(
                "Call AddFlowMapper() during startup before using .Map<TDest>()");
        return _mapper.GetMapper<TSource, TDest>();
    }
}
