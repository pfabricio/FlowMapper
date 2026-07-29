using FlowMapper.FullTextSearch;
using Xunit;

namespace FlowMapper.UnitTests.FullTextSearch;

public class FtsSqlInjectorTests
{
    private const string FtsCondition = "FREETEXT((Nome), @term)";

    [Fact]
    public void InjectFtsCondition_NoWhereNoOrderBy_AppendsWhereAtEnd()
    {
        var sql = "SELECT * FROM Produto";
        var result = FtsSqlInjector.InjectFtsCondition(sql, FtsCondition);
        Assert.Equal("SELECT * FROM Produto WHERE FREETEXT((Nome), @term)", result);
    }

    [Fact]
    public void InjectFtsCondition_WithWhereOnly_InsertsAndAfterWhere()
    {
        var sql = "SELECT * FROM Produto WHERE Ativo = 1";
        var result = FtsSqlInjector.InjectFtsCondition(sql, FtsCondition);
        Assert.Equal("SELECT * FROM Produto WHERE Ativo = 1 AND FREETEXT((Nome), @term)", result);
    }

    [Fact]
    public void InjectFtsCondition_WithOrderByOnly_InsertsWhereBeforeOrderBy()
    {
        var sql = "SELECT * FROM Produto ORDER BY Nome";
        var result = FtsSqlInjector.InjectFtsCondition(sql, FtsCondition);
        Assert.Equal("SELECT * FROM Produto WHERE FREETEXT((Nome), @term) ORDER BY Nome", result);
    }

    [Fact]
    public void InjectFtsCondition_WithWhereAndOrderBy_InsertsAndBeforeOrderBy()
    {
        var sql = "SELECT * FROM Produto WHERE Ativo = 1 ORDER BY Nome";
        var result = FtsSqlInjector.InjectFtsCondition(sql, FtsCondition);
        Assert.Equal("SELECT * FROM Produto WHERE Ativo = 1 AND FREETEXT((Nome), @term) ORDER BY Nome", result);
    }

    [Fact]
    public void InjectFtsCondition_WithWhereAndGroupBy_InsertsAndBeforeGroupBy()
    {
        var sql = "SELECT Categoria, COUNT(*) FROM Produto WHERE Ativo = 1 GROUP BY Categoria";
        var result = FtsSqlInjector.InjectFtsCondition(sql, FtsCondition);
        Assert.Equal("SELECT Categoria, COUNT(*) FROM Produto WHERE Ativo = 1 AND FREETEXT((Nome), @term) GROUP BY Categoria", result);
    }

    [Fact]
    public void InjectFtsCondition_WithWhereAndGroupByAndOrderBy_InsertsAndBeforeGroupBy()
    {
        var sql = "SELECT Categoria, COUNT(*) FROM Produto WHERE Ativo = 1 GROUP BY Categoria ORDER BY Categoria";
        var result = FtsSqlInjector.InjectFtsCondition(sql, FtsCondition);
        Assert.Equal("SELECT Categoria, COUNT(*) FROM Produto WHERE Ativo = 1 AND FREETEXT((Nome), @term) GROUP BY Categoria ORDER BY Categoria", result);
    }

    [Fact]
    public void InjectFtsCondition_WithWhereAndLimit_InsertsAndBeforeLimit()
    {
        var sql = "SELECT * FROM Produto WHERE Ativo = 1 LIMIT 10";
        var result = FtsSqlInjector.InjectFtsCondition(sql, FtsCondition);
        Assert.Equal("SELECT * FROM Produto WHERE Ativo = 1 AND FREETEXT((Nome), @term) LIMIT 10", result);
    }

    [Fact]
    public void InjectFtsCondition_NoWhereWithLimit_InsertsWhereBeforeLimit()
    {
        var sql = "SELECT * FROM Produto LIMIT 10";
        var result = FtsSqlInjector.InjectFtsCondition(sql, FtsCondition);
        Assert.Equal("SELECT * FROM Produto WHERE FREETEXT((Nome), @term) LIMIT 10", result);
    }

    [Fact]
    public void InjectFtsCondition_WithOffsetAndFetch_InsertsWhereBeforeOffset()
    {
        var sql = "SELECT * FROM Produto ORDER BY Nome OFFSET 10 ROWS FETCH NEXT 20 ROWS ONLY";
        var result = FtsSqlInjector.InjectFtsCondition(sql, FtsCondition);
        Assert.Equal("SELECT * FROM Produto WHERE FREETEXT((Nome), @term) ORDER BY Nome OFFSET 10 ROWS FETCH NEXT 20 ROWS ONLY", result);
    }

    [Fact]
    public void InjectFtsCondition_WithSubqueryInWhere_InsertsAfterTopLevelWhere()
    {
        var sql = "SELECT * FROM Produto WHERE Id IN (SELECT ProdutoId FROM Pedidos WHERE Ativo = 1) ORDER BY Nome";
        var result = FtsSqlInjector.InjectFtsCondition(sql, FtsCondition);
        Assert.Equal("SELECT * FROM Produto WHERE Id IN (SELECT ProdutoId FROM Pedidos WHERE Ativo = 1) AND FREETEXT((Nome), @term) ORDER BY Nome", result);
    }

    [Fact]
    public void InjectFtsCondition_WithJoinAndWhere_InsertsAndAfterWhere()
    {
        var sql = "SELECT p.*, tp.Descricao FROM Produto p JOIN TipoProduto tp ON p.IdTipoProduto = tp.Id WHERE p.Ativo = 1 ORDER BY p.Nome";
        var result = FtsSqlInjector.InjectFtsCondition(sql, FtsCondition);
        Assert.Equal("SELECT p.*, tp.Descricao FROM Produto p JOIN TipoProduto tp ON p.IdTipoProduto = tp.Id WHERE p.Ativo = 1 AND FREETEXT((Nome), @term) ORDER BY p.Nome", result);
    }

    [Fact]
    public void InjectFtsCondition_WhereWithParenCondition_InsertsAfterWhereContent()
    {
        var sql = "SELECT * FROM Produto WHERE (Ativo = 1 OR Status = 'Ativo') ORDER BY Nome";
        var result = FtsSqlInjector.InjectFtsCondition(sql, FtsCondition);
        Assert.Equal("SELECT * FROM Produto WHERE (Ativo = 1 OR Status = 'Ativo') AND FREETEXT((Nome), @term) ORDER BY Nome", result);
    }

    [Fact]
    public void InjectFtsCondition_KeywordInStringLiteral_DoesNotMatch()
    {
        var sql = "SELECT * FROM Produto WHERE Status = 'WHERE is not a keyword here' ORDER BY Nome";
        var result = FtsSqlInjector.InjectFtsCondition(sql, FtsCondition);
        Assert.Equal("SELECT * FROM Produto WHERE Status = 'WHERE is not a keyword here' AND FREETEXT((Nome), @term) ORDER BY Nome", result);
    }

    [Fact]
    public void InjectFtsCondition_WhereInsideQuotedIdentifier_DoesNotMatch()
    {
        var sql = "SELECT * FROM \"SomeWHERE\" ORDER BY Nome";
        var result = FtsSqlInjector.InjectFtsCondition(sql, FtsCondition);
        Assert.Equal("SELECT * FROM \"SomeWHERE\" WHERE FREETEXT((Nome), @term) ORDER BY Nome", result);
    }

    [Fact]
    public void InjectFtsCondition_WithLineComment_IgnoresComments()
    {
        var sql = "SELECT * FROM Produto -- WHERE Ativo = 1" + "\nORDER BY Nome";
        var result = FtsSqlInjector.InjectFtsCondition(sql, FtsCondition);
        Assert.Equal("SELECT * FROM Produto -- WHERE Ativo = 1" + "\n WHERE FREETEXT((Nome), @term) ORDER BY Nome", result);
    }

    [Fact]
    public void InjectFtsCondition_WithBlockComment_IgnoresComments()
    {
        var sql = "SELECT * FROM Produto /* WHERE Ativo = 1 */ ORDER BY Nome";
        var result = FtsSqlInjector.InjectFtsCondition(sql, FtsCondition);
        Assert.Equal("SELECT * FROM Produto /* WHERE Ativo = 1 */ WHERE FREETEXT((Nome), @term) ORDER BY Nome", result);
    }

    [Fact]
    public void InjectFtsCondition_WithUnion_InsertsBeforeUnion()
    {
        var sql = "SELECT * FROM Produto WHERE Ativo = 1 UNION ALL SELECT * FROM ProdutoHistorico WHERE Ativo = 1";
        var result = FtsSqlInjector.InjectFtsCondition(sql, FtsCondition);
        Assert.Equal("SELECT * FROM Produto WHERE Ativo = 1 AND FREETEXT((Nome), @term) UNION ALL SELECT * FROM ProdutoHistorico WHERE Ativo = 1", result);
    }

    [Fact]
    public void InjectFtsCondition_WithForClause_InsertsBeforeFor()
    {
        var sql = "SELECT * FROM Produto WHERE Ativo = 1 FOR XML PATH";
        var result = FtsSqlInjector.InjectFtsCondition(sql, FtsCondition);
        Assert.Equal("SELECT * FROM Produto WHERE Ativo = 1 AND FREETEXT((Nome), @term) FOR XML PATH", result);
    }

    [Fact]
    public void InjectFtsCondition_ComplexQueryWithMultipleSubqueries()
    {
        var sql = "SELECT p.*, (SELECT COUNT(*) FROM Pedidos WHERE Pedidos.ProdutoId = p.Id) AS TotalPedidos FROM Produto p WHERE p.Ativo = 1 AND p.Categoria IN (SELECT Id FROM Categorias WHERE Nome LIKE '%urgente%') ORDER BY p.Nome";
        var result = FtsSqlInjector.InjectFtsCondition(sql, FtsCondition);
        Assert.Equal("SELECT p.*, (SELECT COUNT(*) FROM Pedidos WHERE Pedidos.ProdutoId = p.Id) AS TotalPedidos FROM Produto p WHERE p.Ativo = 1 AND p.Categoria IN (SELECT Id FROM Categorias WHERE Nome LIKE '%urgente%') AND FREETEXT((Nome), @term) ORDER BY p.Nome", result);
    }

    [Fact]
    public void InjectFtsCondition_EmptySql_InsertsAtPositionZero()
    {
        var sql = "";
        var result = FtsSqlInjector.InjectFtsCondition(sql, FtsCondition);
        Assert.Equal(" WHERE FREETEXT((Nome), @term)", result);
    }

    [Fact]
    public void InjectFtsCondition_CaseInsensitiveKeywordMatching()
    {
        var sql = "select * from produto where ativo = 1 order by nome";
        var result = FtsSqlInjector.InjectFtsCondition(sql, FtsCondition);
        Assert.Equal("select * from produto where ativo = 1 AND FREETEXT((Nome), @term) order by nome", result);
    }

    [Fact]
    public void InjectFtsCondition_NoWhereWithHaving_InsertsWhereBeforeHaving()
    {
        var sql = "SELECT Categoria, COUNT(*) FROM Produto GROUP BY Categoria HAVING COUNT(*) > 1";
        var result = FtsSqlInjector.InjectFtsCondition(sql, FtsCondition);
        Assert.Equal("SELECT Categoria, COUNT(*) FROM Produto WHERE FREETEXT((Nome), @term) GROUP BY Categoria HAVING COUNT(*) > 1", result);
    }
}
