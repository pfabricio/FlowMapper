using FlowMapper.Abstractions;
using FlowMapper.Diagnostics;
using FlowMapper.Diagnostics.Rules;
using Xunit;

namespace FlowMapper.UnitTests.Diagnostics;

public class LikeWildcardRuleTests
{
    private readonly LikeWildcardRule _rule = new();

    [Fact]
    public void Analyze_LikeWithLeadingWildcard_ReturnsWarning()
    {
        var ctx = new QueryContext { Sql = "SELECT * FROM T WHERE Name LIKE '%test%'" };
        var result = _rule.Analyze(ctx).ToList();

        Assert.Single(result);
        Assert.Equal("FM3002", result[0].Code);
        Assert.Equal(DiagnosticSeverity.Warning, result[0].Severity);
    }

    [Fact]
    public void Analyze_LikeWithoutLeadingWildcard_ReturnsEmpty()
    {
        var ctx = new QueryContext { Sql = "SELECT * FROM T WHERE Name LIKE 'test%'" };
        Assert.Empty(_rule.Analyze(ctx));
    }

    [Fact]
    public void Analyze_NoLike_ReturnsEmpty()
    {
        var ctx = new QueryContext { Sql = "SELECT * FROM T WHERE Name = 'test'" };
        Assert.Empty(_rule.Analyze(ctx));
    }

    [Fact]
    public void CanAnalyze_AlwaysTrue()
    {
        Assert.True(_rule.CanAnalyze(new QueryContext { Sql = "" }));
    }
}

public class SelectStarRuleTests
{
    private readonly SelectStarRule _rule = new();

    [Fact]
    public void Analyze_SelectStar_ReturnsInfo()
    {
        var ctx = new QueryContext { Sql = "SELECT * FROM T" };
        var result = _rule.Analyze(ctx).ToList();

        Assert.Single(result);
        Assert.Equal("FM3005", result[0].Code);
        Assert.Equal(DiagnosticSeverity.Info, result[0].Severity);
    }

    [Fact]
    public void Analyze_SelectExplicitColumns_ReturnsEmpty()
    {
        var ctx = new QueryContext { Sql = "SELECT Id, Name FROM T" };
        Assert.Empty(_rule.Analyze(ctx));
    }

    [Fact]
    public void Analyze_NonSelectStatement_ReturnsEmpty()
    {
        var ctx = new QueryContext { Sql = "DELETE FROM T" };
        Assert.Empty(_rule.Analyze(ctx));
    }

    [Fact]
    public void CanAnalyze_AlwaysTrue()
    {
        Assert.True(_rule.CanAnalyze(new QueryContext { Sql = "" }));
    }
}

public class OrderByIndexRuleTests
{
    private readonly OrderByIndexRule _rule = new();

    [Fact]
    public void Analyze_OrderByColumn_ReturnsWarning()
    {
        var ctx = new QueryContext { Sql = "SELECT * FROM T ORDER BY Name" };
        var result = _rule.Analyze(ctx).ToList();

        Assert.Single(result);
        Assert.Equal("FM3003", result[0].Code);
        Assert.Equal(DiagnosticSeverity.Warning, result[0].Severity);
        Assert.Equal("Name", result[0].Column);
    }

    [Fact]
    public void Analyze_NoOrderBy_ReturnsEmpty()
    {
        var ctx = new QueryContext { Sql = "SELECT * FROM T WHERE Id = 1" };
        Assert.Empty(_rule.Analyze(ctx));
    }

    [Fact]
    public void CanAnalyze_OrderByInSubquery_ReturnsFalse()
    {
        var ctx = new QueryContext { Sql = "SELECT * FROM (SELECT * FROM T ORDER BY Id) t WHERE t.Name = 'a'" };
        Assert.False(_rule.CanAnalyze(ctx));
    }

    [Fact]
    public void CanAnalyze_WithOrderBy_ReturnsTrue()
    {
        var ctx = new QueryContext { Sql = "SELECT * FROM T ORDER BY Id" };
        Assert.True(_rule.CanAnalyze(ctx));
    }

    [Fact]
    public void CanAnalyze_WithoutOrderBy_ReturnsFalse()
    {
        var ctx = new QueryContext { Sql = "SELECT * FROM T" };
        Assert.False(_rule.CanAnalyze(ctx));
    }

    [Fact]
    public void CanAnalyze_OrderByInStringLiteral_ReturnsFalse()
    {
        var ctx = new QueryContext { Sql = "SELECT * FROM T WHERE Name = 'ORDER BY test'" };
        Assert.False(_rule.CanAnalyze(ctx));
    }
}

public class LargeOffsetRuleTests
{
    private readonly LargeOffsetRule _rule = new();

    [Fact]
    public void Analyze_LargeOffsetNoWhere_ReturnsWarning()
    {
        var ctx = new QueryContext { Sql = "SELECT * FROM T ORDER BY Id OFFSET 2000 ROWS FETCH NEXT 10 ROWS ONLY" };
        var result = _rule.Analyze(ctx).ToList();

        Assert.Single(result);
        Assert.Equal("FM3006", result[0].Code);
        Assert.Equal(DiagnosticSeverity.Warning, result[0].Severity);
    }

    [Fact]
    public void Analyze_LargeOffsetWithWhere_ReturnsEmpty()
    {
        var ctx = new QueryContext { Sql = "SELECT * FROM T WHERE Status = 1 ORDER BY Id OFFSET 2000 ROWS FETCH NEXT 10 ROWS ONLY" };
        Assert.Empty(_rule.Analyze(ctx));
    }

    [Fact]
    public void Analyze_SmallOffset_ReturnsEmpty()
    {
        var ctx = new QueryContext { Sql = "SELECT * FROM T ORDER BY Id OFFSET 100 ROWS FETCH NEXT 10 ROWS ONLY" };
        Assert.Empty(_rule.Analyze(ctx));
    }

    [Fact]
    public void Analyze_NoOffset_ReturnsEmpty()
    {
        var ctx = new QueryContext { Sql = "SELECT * FROM T" };
        Assert.Empty(_rule.Analyze(ctx));
    }

    [Fact]
    public void CanAnalyze_WithOffset_ReturnsTrue()
    {
        var ctx = new QueryContext { Sql = "SELECT * FROM T OFFSET 10 ROWS" };
        Assert.True(_rule.CanAnalyze(ctx));
    }

    [Fact]
    public void CanAnalyze_WithoutOffset_ReturnsFalse()
    {
        var ctx = new QueryContext { Sql = "SELECT * FROM T" };
        Assert.False(_rule.CanAnalyze(ctx));
    }
}

public class CartesianJoinRuleTests
{
    private readonly CartesianJoinRule _rule = new();

    [Fact]
    public void Analyze_JoinWithoutOn_ReturnsWarning()
    {
        var ctx = new QueryContext { Sql = "SELECT * FROM T1 JOIN T2 ON T1.Id = T2.Id, T3 JOIN T4" };
        var result = _rule.Analyze(ctx).ToList();

        Assert.Single(result);
        Assert.Equal("FM3007", result[0].Code);
        Assert.Equal(DiagnosticSeverity.Warning, result[0].Severity);
    }

    [Fact]
    public void Analyze_AllJoinsWithOn_ReturnsEmpty()
    {
        var ctx = new QueryContext { Sql = "SELECT * FROM T1 JOIN T2 ON T1.Id = T2.Id JOIN T3 ON T2.Id = T3.Id" };
        Assert.Empty(_rule.Analyze(ctx));
    }

    [Fact]
    public void Analyze_NoJoins_ReturnsEmpty()
    {
        var ctx = new QueryContext { Sql = "SELECT * FROM T1, T2" };
        Assert.Empty(_rule.Analyze(ctx));
    }

    [Fact]
    public void CanAnalyze_WithJoin_ReturnsTrue()
    {
        var ctx = new QueryContext { Sql = "SELECT * FROM T1 JOIN T2 ON T1.Id = T2.Id" };
        Assert.True(_rule.CanAnalyze(ctx));
    }

    [Fact]
    public void CanAnalyze_WithoutJoin_ReturnsFalse()
    {
        var ctx = new QueryContext { Sql = "SELECT * FROM T1" };
        Assert.False(_rule.CanAnalyze(ctx));
    }
}
