using FlowMapper.Abstractions;
using FlowMapper.FullTextSearch;

namespace FlowMapper.Diagnostics;

public interface ISchemaInspector
{
    FtsIndexState VerifyIndex(string table, string column, IDatabaseProvider provider);
    void ClearCache();
}
