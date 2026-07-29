using System.Text.RegularExpressions;

namespace FlowMapper.Diagnostics.Rules;

public partial class OrderByIndexRule : IDiagnosticRule
{
    [GeneratedRegex(@"ORDER\s+BY\s+(\S+)", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex OrderByPattern();

    public bool CanAnalyze(QueryContext context) => ContainsOrderBy(context.Sql);

    public IEnumerable<Diagnostic> Analyze(QueryContext context)
    {
        var match = OrderByPattern().Match(context.Sql);
        if (match.Success)
        {
            var column = match.Groups[1].Value.TrimEnd(',');
            yield return new Diagnostic("FM3003", DiagnosticSeverity.Warning,
                $"ORDER BY on '{column}' without index confirmation. Verify index exists for performance.",
                Column: column,
                Source: DiagnosticSource.Runtime);
        }
    }

    private static bool ContainsOrderBy(string sql)
    {
        int depth = 0;
        bool inString = false;
        for (int i = 0; i < sql.Length; i++)
        {
            var c = sql[i];
            if (c == '\'') { inString = !inString; continue; }
            if (inString) continue;
            if (c == '(') depth++;
            else if (c == ')') depth--;
            if (depth != 0) continue;
            if (i + 8 < sql.Length &&
                char.ToUpperInvariant(sql[i]) == 'O' &&
                char.ToUpperInvariant(sql[i + 1]) == 'R' &&
                sql[i + 2] == 'D' &&
                sql[i + 3] == 'E' &&
                sql[i + 4] == 'R' &&
                sql[i + 5] == ' ' &&
                char.ToUpperInvariant(sql[i + 6]) == 'B' &&
                char.ToUpperInvariant(sql[i + 7]) == 'Y')
                return true;
        }
        return false;
    }
}
