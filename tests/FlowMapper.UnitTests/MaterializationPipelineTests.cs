using System.Data;
using FlowMapper.Execution.Artifacts;
using FlowMapper.Materializer.Pipeline;
using Xunit;

namespace FlowMapper.UnitTests;

public class MaterializationPipelineTests
{
    private sealed class TestDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public decimal? Price { get; set; }
    }

    private static IDataReader CreateReader(params (string name, object? value)[] columns)
    {
        var table = new DataTable();
        foreach (var (name, _) in columns)
            table.Columns.Add(name);
        var row = table.NewRow();
        for (var i = 0; i < columns.Length; i++)
            row[i] = columns[i].value ?? DBNull.Value;
        table.Rows.Add(row);
        return table.CreateDataReader();
    }

    [Fact]
    public void Pipeline_MaterializesSimpleType_FromDataReader()
    {
        var artifact = new MaterializationArtifact(
            Name: "Test",
            Version: new Version(2, 0),
            TargetType: typeof(TestDto),
            Separator: "_",
            ConstructorDelegate: null,
            ColumnBindings:
            [
                new ColumnBinding("Id", "Id", typeof(int), Converter: null, IsNullable: false),
                new ColumnBinding("Name", "Name", typeof(string), Converter: null, IsNullable: true),
                new ColumnBinding("Price", "Price", typeof(decimal?), Converter: null, IsNullable: true)
            ],
            MaterializationDelegate: null
        );

        using var reader = CreateReader(("Id", 42), ("Name", "Alice"), ("Price", 19.99m));

        var pipeline = new MaterializationPipeline();
        reader.Read();

        var result = pipeline.Materialize<TestDto>(reader, artifact);

        Assert.NotNull(result);
        Assert.Equal(42, result.Id);
        Assert.Equal("Alice", result.Name);
        Assert.Equal(19.99m, result.Price);
    }

    [Fact]
    public void Pipeline_HandlesNullValues_ForNullableProperties()
    {
        var artifact = new MaterializationArtifact(
            Name: "Test",
            Version: new Version(2, 0),
            TargetType: typeof(TestDto),
            Separator: "_",
            ConstructorDelegate: null,
            ColumnBindings:
            [
                new ColumnBinding("Id", "Id", typeof(int), Converter: null, IsNullable: false),
                new ColumnBinding("Name", "Name", typeof(string), Converter: null, IsNullable: true),
                new ColumnBinding("Price", "Price", typeof(decimal?), Converter: null, IsNullable: true)
            ],
            MaterializationDelegate: null
        );

        using var reader = CreateReader(("Id", 1), ("Name", DBNull.Value), ("Price", DBNull.Value));

        var pipeline = new MaterializationPipeline();
        reader.Read();

        var result = pipeline.Materialize<TestDto>(reader, artifact);

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("", result.Name);  // default value, nulls are skipped
        Assert.Null(result.Price);
    }

    [Fact]
    public void Pipeline_MaterializeAll_YieldsMultipleRows()
    {
        var artifact = new MaterializationArtifact(
            Name: "Test",
            Version: new Version(2, 0),
            TargetType: typeof(TestDto),
            Separator: "_",
            ConstructorDelegate: null,
            ColumnBindings:
            [
                new ColumnBinding("Id", "Id", typeof(int), Converter: null, IsNullable: false),
                new ColumnBinding("Name", "Name", typeof(string), Converter: null, IsNullable: true)
            ],
            MaterializationDelegate: null
        );

        var table = new DataTable();
        table.Columns.Add("Id", typeof(int));
        table.Columns.Add("Name", typeof(string));
        table.Rows.Add(1, "Alice");
        table.Rows.Add(2, "Bob");
        using var reader = table.CreateDataReader();

        var pipeline = new MaterializationPipeline();
        var results = pipeline.MaterializeAll<TestDto>(reader, artifact).ToList();

        Assert.Equal(2, results.Count);
        Assert.Equal("Alice", results[0].Name);
        Assert.Equal("Bob", results[1].Name);
    }

    [Fact]
    public void Pipeline_WithCustomConstructorDelegate_UsesIt()
    {
        Func<IDataReader, TestDto> factory = _ => new TestDto { Id = 999 };

        var artifact = new MaterializationArtifact(
            Name: "Test",
            Version: new Version(2, 0),
            TargetType: typeof(TestDto),
            Separator: "_",
            ConstructorDelegate: factory,
            ColumnBindings:
            [
                new ColumnBinding("Name", "Name", typeof(string), Converter: null, IsNullable: true)
            ],
            MaterializationDelegate: null
        );

        using var reader = CreateReader(("Id", 42), ("Name", "Alice"));

        var pipeline = new MaterializationPipeline();
        reader.Read();

        var result = pipeline.Materialize<TestDto>(reader, artifact);

        Assert.Equal(999, result.Id);  // from factory
        Assert.Equal("Alice", result.Name);  // from binding
    }
}
