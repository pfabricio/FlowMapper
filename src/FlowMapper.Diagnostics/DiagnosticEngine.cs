namespace FlowMapper.Diagnostics;

public class DiagnosticEngine
{
    private readonly IEnumerable<IDiagnosticRule> _rules;
    private readonly IDiagnosticCollector _collector;

    public DiagnosticEngine(IEnumerable<IDiagnosticRule> rules, IDiagnosticCollector collector)
    {
        _rules = rules;
        _collector = collector;
    }

    public void Analyze(QueryContext context)
    {
        foreach (var rule in _rules)
        {
            if (!rule.CanAnalyze(context))
                continue;

            foreach (var diagnostic in rule.Analyze(context))
                _collector.Emit(diagnostic);
        }
    }
}
