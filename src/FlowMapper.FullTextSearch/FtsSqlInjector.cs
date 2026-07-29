namespace FlowMapper.FullTextSearch;

public static class FtsSqlInjector
{
    public static string InjectFtsCondition(string sql, string ftsCondition)
    {
        var scan = ScanClauses(sql);

        if (scan.WherePos >= 0)
        {
            var cut = scan.WhereContentEnd;
            if (cut > 0 && sql[cut - 1] == ' ')
                cut--;
            var left = sql[..cut];
            var right = sql[scan.WhereContentEnd..];
            return $"{left} AND {ftsCondition} {right}".TrimEnd();
        }

        var pos = scan.FirstClausePos >= 0 ? scan.FirstClausePos : sql.Length;
        if (pos > 0 && sql[pos - 1] == ' ')
            pos--;
        var before = sql[..pos];
        var after = scan.FirstClausePos >= 0 ? sql[scan.FirstClausePos..] : "";
        return $"{before} WHERE {ftsCondition} {after}".TrimEnd();
    }

    private static (int WherePos, int WhereContentEnd, int FirstClausePos) ScanClauses(string sql)
    {
        int wherePos = -1;
        int whereContentEnd = -1;
        int firstClausePos = -1;
        int depth = 0;
        bool inSingleQuote = false, inDoubleQuote = false;
        bool inLineComment = false, inBlockComment = false;

        for (int i = 0; i < sql.Length; i++)
        {
            var c = sql[i];

            if (!inSingleQuote && !inDoubleQuote && !inBlockComment)
            {
                if (c == '-' && i + 1 < sql.Length && sql[i + 1] == '-')
                { inLineComment = true; i++; continue; }

                if (c == '/' && i + 1 < sql.Length && sql[i + 1] == '*')
                { inBlockComment = true; i++; continue; }
            }

            if (inLineComment)
            {
                if (c == '\n') inLineComment = false;
                continue;
            }

            if (inBlockComment)
            {
                if (c == '*' && i + 1 < sql.Length && sql[i + 1] == '/')
                { inBlockComment = false; i++; }
                continue;
            }

            if (!inSingleQuote && !inDoubleQuote && (c == '\'' || c == '"'))
            { if (c == '\'') inSingleQuote = true; else inDoubleQuote = true; continue; }

            if (inSingleQuote && c == '\'') { inSingleQuote = false; continue; }
            if (inDoubleQuote && c == '"') { inDoubleQuote = false; continue; }
            if (inSingleQuote || inDoubleQuote) continue;

            if (c == '(') depth++;
            else if (c == ')') depth--;
            if (depth != 0) continue;

            if (wherePos < 0 && MatchKeyword(sql, i, "WHERE"))
            {
                wherePos = i;
                i += 5;
                continue;
            }

            if (firstClausePos < 0 && MatchAnyClauseKeyword(sql, i, out _))
            {
                firstClausePos = i;
                if (wherePos >= 0 && whereContentEnd < 0)
                    whereContentEnd = i;
            }
        }

        if (wherePos >= 0 && whereContentEnd < 0)
            whereContentEnd = sql.Length;
        if (firstClausePos < 0)
            firstClausePos = -1;

        return (wherePos, whereContentEnd, firstClausePos);
    }

    private static readonly string[] ClauseKeywords =
        ["ORDER BY", "GROUP BY", "HAVING", "LIMIT", "OFFSET", "FETCH",
         "FOR", "UNION", "INTERSECT", "EXCEPT", "OPTION"];

    private static bool MatchKeyword(string sql, int pos, string keyword)
    {
        if (pos + keyword.Length > sql.Length) return false;
        for (int i = 0; i < keyword.Length; i++)
            if (char.ToUpperInvariant(sql[pos + i]) != keyword[i]) return false;
        if (pos > 0 && IsIdent(sql[pos - 1])) return false;
        int after = pos + keyword.Length;
        if (after < sql.Length && IsIdent(sql[after])) return false;
        return true;
    }

    private static bool MatchAnyClauseKeyword(string sql, int pos, out int length)
    {
        foreach (var kw in ClauseKeywords)
        {
            if (pos + kw.Length > sql.Length) continue;
            bool ok = true;
            for (int i = 0; i < kw.Length; i++)
                if (char.ToUpperInvariant(sql[pos + i]) != kw[i]) { ok = false; break; }
            if (!ok) continue;
            if (pos > 0 && IsIdent(sql[pos - 1])) continue;
            int after = pos + kw.Length;
            if (after < sql.Length && IsIdent(sql[after])) continue;
            length = kw.Length;
            return true;
        }
        length = 0;
        return false;
    }

    private static bool IsIdent(char c) => char.IsLetterOrDigit(c) || c == '_' || c == '$';
}
