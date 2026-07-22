namespace FlowMapper.Abstractions;

public enum ExecutionPhase
{
    BeforeExecute,
    Execute,
    Mapping,
    RowRead,
    AfterExecute,
    Completed
}
