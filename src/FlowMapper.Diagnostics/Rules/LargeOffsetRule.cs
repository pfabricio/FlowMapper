using System.Text.RegularExpressions;

namespace FlowMapper.Diagnostics.Rules;

public partial class LargeOffsetRule : IDiagnosticRule
{
    [GeneratedRegex(@"OFFSET\s+(\d+)", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex OffsetPattern();

    [GeneratedRegex(@"\bWHERE\b", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex WherePattern();

    public bool CanAnalyze(QueryContext context) => OffsetPattern().IsMatch(context.Sql);

    public IEnumerable<Diagnostic> Analyze(QueryContext context)
    {
        var match = OffsetPattern().Match(context.Sql);
        if (match.Success && int.TryParse(match.Groups[1].Value, out var offset) && offset > 1000)
        {
            var hasWhere = WherePattern().IsMatch(context.Sql);
            if (!hasWhere)
            {
                yield return new Diagnostic("FM3006", DiagnosticSeverity.Warning,
                    $"Large OFFSET ({offset}) without WHERE clause may scan many rows. Consider adding filters.",
                    Source: DiagnosticSource.Runtime);
            }
        }
    }
}
