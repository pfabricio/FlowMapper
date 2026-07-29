using System.Text.RegularExpressions;

namespace FlowMapper.Diagnostics.Rules;

public partial class CartesianJoinRule : IDiagnosticRule
{
    [GeneratedRegex(@"\bJOIN\s+\S+\s+ON\s+\S+\s*=\s*\S+", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex JoinWithOnPattern();

    [GeneratedRegex(@"\bJOIN\b", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex JoinPattern();

    public bool CanAnalyze(QueryContext context) => JoinPattern().IsMatch(context.Sql);

    public IEnumerable<Diagnostic> Analyze(QueryContext context)
    {
        var joinCount = JoinPattern().Matches(context.Sql).Count;
        var onCount = JoinWithOnPattern().Matches(context.Sql).Count;

        if (joinCount > onCount)
        {
            yield return new Diagnostic("FM3007", DiagnosticSeverity.Warning,
                $"JOIN without ON condition detected ({joinCount - onCount} missing). This produces a Cartesian product.",
                Source: DiagnosticSource.Runtime);
        }
    }
}
