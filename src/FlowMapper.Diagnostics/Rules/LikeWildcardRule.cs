using System.Text.RegularExpressions;

namespace FlowMapper.Diagnostics.Rules;

public partial class LikeWildcardRule : IDiagnosticRule
{
    [GeneratedRegex(@"LIKE\s+'%[^']+%'", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex LikeWildcardPattern();

    public bool CanAnalyze(QueryContext context) => true;

    public IEnumerable<Diagnostic> Analyze(QueryContext context)
    {
        if (LikeWildcardPattern().IsMatch(context.Sql))
        {
            yield return new Diagnostic("FM3002", DiagnosticSeverity.Warning,
                "LIKE with leading wildcard '%...%' may cause table scan. Consider Full-Text Search instead.",
                Source: DiagnosticSource.Runtime);
        }
    }
}
