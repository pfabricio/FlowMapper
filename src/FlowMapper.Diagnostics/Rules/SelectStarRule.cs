namespace FlowMapper.Diagnostics.Rules;

public class SelectStarRule : IDiagnosticRule
{
    public bool CanAnalyze(QueryContext context) => true;

    public IEnumerable<Diagnostic> Analyze(QueryContext context)
    {
        var trimmed = context.Sql.TrimStart();
        if (trimmed.Length > 6 &&
            trimmed[0] == 'S' && trimmed[1] == 'E' && trimmed[2] == 'L' &&
            trimmed[3] == 'E' && trimmed[4] == 'C' && trimmed[5] == 'T' &&
            trimmed[6] == ' ')
        {
            var afterSelect = trimmed[7..].TrimStart();
            if (afterSelect.Length > 0 && afterSelect[0] == '*')
            {
                yield return new Diagnostic("FM3005", DiagnosticSeverity.Info,
                    "SELECT * detected. Prefer explicit column names for performance and clarity.",
                    Source: DiagnosticSource.Runtime);
            }
        }
    }
}
