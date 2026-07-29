namespace FlowMapper.Diagnostics;

public interface IDiagnosticRule
{
    bool CanAnalyze(QueryContext context);
    IEnumerable<Diagnostic> Analyze(QueryContext context);
}
