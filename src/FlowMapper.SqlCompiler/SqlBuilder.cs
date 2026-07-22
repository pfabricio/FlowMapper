using System.Text;

namespace FlowMapper.SqlCompiler;

public sealed class SqlBuilder
{
    private enum SqlOperation { Select, Insert, Update, Delete }

    private readonly StringBuilder _sql = new();
    private readonly List<string> _columns = new();
    private readonly List<string> _from = new();
    private readonly List<string> _where = new();
    private readonly List<string> _orderBy = new();
    private readonly List<string> _joins = new();
    private readonly List<string> _setClauses = new();
    private string? _tableName;
    private string? _top;
    private readonly SqlOperation _operation;

    private SqlBuilder(SqlOperation operation)
    {
        _operation = operation;
    }

    public static SqlBuilder Select(params string[] columns)
    {
        var builder = new SqlBuilder(SqlOperation.Select);
        builder._columns.AddRange(columns.Length > 0 ? columns : ["*"]);
        return builder;
    }

    public static SqlBuilder InsertInto(string table)
    {
        var builder = new SqlBuilder(SqlOperation.Insert) { _tableName = table };
        return builder;
    }

    public static SqlBuilder Update(string table)
    {
        var builder = new SqlBuilder(SqlOperation.Update) { _tableName = table };
        return builder;
    }

    public static SqlBuilder DeleteFrom(string table)
    {
        var builder = new SqlBuilder(SqlOperation.Delete) { _tableName = table };
        return builder;
    }

    public SqlBuilder From(string table)
    {
        _from.Add(table);
        return this;
    }

    public SqlBuilder Where(string condition)
    {
        _where.Add(condition);
        return this;
    }

    public SqlBuilder And(string condition)
    {
        _where.Add(condition);
        return this;
    }

    public SqlBuilder Join(string join)
    {
        _joins.Add($"JOIN {join}");
        return this;
    }

    public SqlBuilder LeftJoin(string join)
    {
        _joins.Add($"LEFT JOIN {join}");
        return this;
    }

    public SqlBuilder OrderBy(string column, bool descending = false)
    {
        _orderBy.Add(descending ? $"{column} DESC" : column);
        return this;
    }

    public SqlBuilder Set(string column, string parameterName)
    {
        _setClauses.Add($"{column} = @{parameterName}");
        return this;
    }

    public SqlBuilder Top(int n)
    {
        _top = n.ToString();
        return this;
    }

    public string Build()
    {
        return _operation switch
        {
            SqlOperation.Select => BuildSelect(),
            SqlOperation.Insert => BuildInsert(),
            SqlOperation.Update => BuildUpdate(),
            SqlOperation.Delete => BuildDelete(),
            _ => _sql.ToString()
        };
    }

    private string BuildSelect()
    {
        _sql.Append("SELECT ");

        if (_top != null)
            _sql.Append($"TOP {_top} ");

        _sql.Append(string.Join(", ", _columns));

        if (_from.Count > 0)
            _sql.Append($" FROM {string.Join(", ", _from)}");

        foreach (var join in _joins)
            _sql.Append($" {join}");

        if (_where.Count > 0)
            _sql.Append($" WHERE {string.Join(" AND ", _where)}");

        if (_orderBy.Count > 0)
            _sql.Append($" ORDER BY {string.Join(", ", _orderBy)}");

        return _sql.ToString();
    }

    private string BuildInsert()
    {
        var cols = new List<string>();
        var pars = new List<string>();

        foreach (var clause in _setClauses)
        {
            var eqPos = clause.IndexOf('=');
            if (eqPos < 0) continue;

            cols.Add(clause[..eqPos].Trim());

            var paramPart = clause[(eqPos + 1)..].Trim();
            pars.Add($"@{paramPart.TrimStart('@')}");
        }

        return $"INSERT INTO {_tableName} ({string.Join(", ", cols)}) VALUES ({string.Join(", ", pars)})";
    }

    private string BuildUpdate()
    {
        _sql.Append($"UPDATE {_tableName} SET {string.Join(", ", _setClauses)}");

        if (_where.Count > 0)
            _sql.Append($" WHERE {string.Join(" AND ", _where)}");

        return _sql.ToString();
    }

    private string BuildDelete()
    {
        _sql.Append($"DELETE FROM {_tableName}");

        if (_where.Count > 0)
            _sql.Append($" WHERE {string.Join(" AND ", _where)}");

        return _sql.ToString();
    }

    public override string ToString() => Build();
}
