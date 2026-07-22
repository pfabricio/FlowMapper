using FlowMapper.Execution.Artifacts;
using FlowMapper.SqlCompiler;
using FlowMapper.SqlCompiler.Pipeline;
using FlowMapper.SqlCompiler.Pipeline.Middlewares;
using Xunit;

namespace FlowMapper.UnitTests;

public class SqlCompilerTests
{
    [Fact]
    public void SqlPipeline_CompilesRawSql()
    {
        var pipeline = new SqlPipeline();

        var result = pipeline.Compile("SELECT * FROM Users");

        Assert.Equal("SELECT * FROM Users", result.CommandText);
        Assert.Empty(result.Parameters);
    }

    [Fact]
    public void SqlPipeline_CompilesWithParameters()
    {
        var pipeline = new SqlPipeline();

        var result = pipeline.Compile("SELECT * FROM Users WHERE Id = @Id", new { Id = 1 });

        Assert.Contains("@Id", result.CommandText);
        Assert.Single(result.Parameters);
        Assert.Equal("Id", result.Parameters.First().Name);
    }

    [Fact]
    public void SqlPipeline_WithMiddleware_TransformsResult()
    {
        var pipeline = new SqlPipeline(
            [new TestMiddleware()]);

        var result = pipeline.Compile("SELECT * FROM Users");

        Assert.StartsWith("TRANSFORMED:", result.CommandText);
    }

    [Fact]
    public void SqlPipeline_CompilesFromArtifact()
    {
        var artifact = new SqlArtifact(
            Name: "GetUsers",
            Version: new Version(2, 0),
            CommandText: "SELECT Id, Name FROM Users WHERE Active = @Active",
            CommandKind: Execution.Artifacts.CommandType.Query,
            Parameters:
            [
                new ParameterBinding("Active", typeof(bool), null)
            ],
            ExecutionDelegate: null
        );

        var pipeline = new SqlPipeline();
        var result = pipeline.Compile(artifact);

        Assert.Equal("SELECT Id, Name FROM Users WHERE Active = @Active", result.CommandText);
        Assert.Single(result.Parameters);
    }

    [Fact]
    public void SqlBuilder_Select_GeneratesCorrectSql()
    {
        var sql = SqlBuilder
            .Select("Id", "Name")
            .From("Users")
            .Where("Active = @Active")
            .OrderBy("Name")
            .Build();

        Assert.Equal("SELECT Id, Name FROM Users WHERE Active = @Active ORDER BY Name", sql);
    }

    [Fact]
    public void SqlBuilder_SelectStar_GeneratesCorrectSql()
    {
        var sql = SqlBuilder
            .Select()
            .From("Products")
            .Build();

        Assert.Equal("SELECT * FROM Products", sql);
    }

    [Fact]
    public void SqlBuilder_Insert_GeneratesCorrectSql()
    {
        var sql = SqlBuilder
            .InsertInto("Users")
            .Set("Name", "name")
            .Set("Email", "email")
            .Build();

        Assert.Equal("INSERT INTO Users (Name, Email) VALUES (@name, @email)", sql);
    }

    [Fact]
    public void SqlBuilder_Update_GeneratesCorrectSql()
    {
        var sql = SqlBuilder
            .Update("Users")
            .Set("Name", "name")
            .Where("Id = @Id")
            .Build();

        Assert.Equal("UPDATE Users SET Name = @name WHERE Id = @Id", sql);
    }

    [Fact]
    public void SqlBuilder_Delete_GeneratesCorrectSql()
    {
        var sql = SqlBuilder
            .DeleteFrom("Users")
            .Where("Id = @Id")
            .Build();

        Assert.Equal("DELETE FROM Users WHERE Id = @Id", sql);
    }

    [Fact]
    public void SqlBuilder_Top_GeneratesCorrectSql()
    {
        var sql = SqlBuilder
            .Select("Id", "Name")
            .From("Users")
            .Top(5)
            .Build();

        Assert.Equal("SELECT TOP 5 Id, Name FROM Users", sql);
    }

    [Fact]
    public void SqlBuilder_WithJoin_GeneratesCorrectSql()
    {
        var sql = SqlBuilder
            .Select("u.Id", "o.Total")
            .From("Users u")
            .Join("Orders o ON o.UserId = u.Id")
            .Build();

        Assert.Equal("SELECT u.Id, o.Total FROM Users u JOIN Orders o ON o.UserId = u.Id", sql);
    }

    [Fact]
    public void SqlDelegateBuilder_BuildParameterBinder_ReturnsCompiledDelegate()
    {
        var builder = new SqlDelegateBuilder();
        var binder = builder.BuildParameterBinder(typeof(TestParams));

        var result = (List<IParameterBinding>)binder.DynamicInvoke(new TestParams { Id = 1, Name = "Test" })!;

        Assert.Equal(2, result.Count);
        Assert.Contains(result, p => p.Name == "Id");
        Assert.Contains(result, p => p.Name == "Name");
    }

    private sealed class TestParams
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }

    private sealed class TestMiddleware : ISqlMiddleware
    {
        public CompiledSql Process(string sql, object? parameters, SqlDelegate next)
        {
            var result = next(sql, parameters);
            return new CompiledSql(
                $"TRANSFORMED:{result.CommandText}",
                result.Parameters,
                result.ParameterDelegate);
        }
    }
}
