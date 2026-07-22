using System.Data;
using FlowMapper.Execution.Artifacts;
using FlowMapper.Materializer.Pipeline;
using Xunit;

namespace FlowMapper.UnitTests.Materializer;

public class CascadeTests
{
    private sealed class ClienteDto
    {
        public int Us_Id { get; set; }
        public string? Us_Nome { get; set; }
        public PerfilDto? Perfil { get; set; }
    }

    private sealed class PerfilDto
    {
        public string? Nome { get; set; }
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
    public void Cascade_FlatSempSeparador_MapeiaDiretamente()
    {
        var artifact = new MaterializationArtifact(
            Name: "Test",
            Version: new Version(2, 0),
            TargetType: typeof(ClienteDto),
            Separator: "_",
            ConstructorDelegate: null,
            ColumnBindings:
            [
                new ColumnBinding("Us_Id", "Us_Id", typeof(int), Converter: null, IsNullable: false),
                new ColumnBinding("Us_Nome", "Us_Nome", typeof(string), Converter: null, IsNullable: true)
            ],
            MaterializationDelegate: null
        );

        using var reader = CreateReader(("Us_Id", 1), ("Us_Nome", "João"));

        var pipeline = new MaterializationPipeline();
        reader.Read();

        var result = pipeline.Materialize<ClienteDto>(reader, artifact);

        Assert.NotNull(result);
        Assert.Equal(1, result.Us_Id);
        Assert.Equal("João", result.Us_Nome);
        Assert.Null(result.Perfil);
    }

    [Fact]
    public void Cascade_ComMatch_PreencheNested()
    {
        using var reader = CreateReader(
            ("Us_Id", 1),
            ("Us_Nome", "João"),
            ("Perfil_Nome", "Admin")
        );

        reader.Read();

        var builder = new MaterializationDelegateBuilder();

        var artifact = new MaterializationArtifact(
            Name: "Test",
            Version: new Version(2, 0),
            TargetType: typeof(ClienteDto),
            Separator: "_",
            ConstructorDelegate: null,
            ColumnBindings:
            [
                new ColumnBinding("Us_Id", "Us_Id", typeof(int), Converter: null, IsNullable: false),
                new ColumnBinding("Us_Nome", "Us_Nome", typeof(string), Converter: null, IsNullable: true),
                new ColumnBinding("Perfil_Nome", "Perfil_Nome", typeof(string), Converter: null, IsNullable: true)
            ],
            MaterializationDelegate: null
        );

        var result = builder.BuildDelegate<ClienteDto>(artifact)(reader);

        Assert.NotNull(result);
        Assert.Equal(1, result.Us_Id);
        Assert.Equal("João", result.Us_Nome);
        Assert.NotNull(result.Perfil);
        Assert.Equal("Admin", result.Perfil.Nome);
    }

    [Fact]
    public void Cascade_LeftJoinSemMatch_DeixaNestedNull()
    {
        using var reader = CreateReader(
            ("Us_Id", 1),
            ("Us_Nome", "João"),
            ("Perfil_Nome", DBNull.Value)
        );

        reader.Read();

        var builder = new MaterializationDelegateBuilder();

        var artifact = new MaterializationArtifact(
            Name: "Test",
            Version: new Version(2, 0),
            TargetType: typeof(ClienteDto),
            Separator: "_",
            ConstructorDelegate: null,
            ColumnBindings:
            [
                new ColumnBinding("Us_Id", "Us_Id", typeof(int), Converter: null, IsNullable: false),
                new ColumnBinding("Us_Nome", "Us_Nome", typeof(string), Converter: null, IsNullable: true),
                new ColumnBinding("Perfil_Nome", "Perfil_Nome", typeof(string), Converter: null, IsNullable: true)
            ],
            MaterializationDelegate: null
        );

        var result = builder.BuildDelegate<ClienteDto>(artifact)(reader);

        Assert.NotNull(result);
        Assert.Equal(1, result.Us_Id);
        Assert.Equal("João", result.Us_Nome);
        Assert.Null(result.Perfil);
    }

    [Fact]
    public void Cascade_SeparadorCustom_Funciona()
    {
        var builder = new MaterializationDelegateBuilder(":");

        using var reader = CreateReader(
            ("Us_Id", 1),
            ("Perfil:Nome", "Admin")
        );

        reader.Read();

        var artifact = new MaterializationArtifact(
            Name: "Test",
            Version: new Version(2, 0),
            TargetType: typeof(ClienteDto),
            Separator: ":",
            ConstructorDelegate: null,
            ColumnBindings:
            [
                new ColumnBinding("Us_Id", "Us_Id", typeof(int), Converter: null, IsNullable: false),
                new ColumnBinding("Perfil:Nome", "Perfil:Nome", typeof(string), Converter: null, IsNullable: true)
            ],
            MaterializationDelegate: null
        );

        var result = builder.BuildDelegate<ClienteDto>(artifact)(reader);

        Assert.NotNull(result);
        Assert.Equal(1, result.Us_Id);
        Assert.NotNull(result.Perfil);
        Assert.Equal("Admin", result.Perfil.Nome);
    }

    [Fact]
    public void Cascade_PlanRecursivo_GeraBindingsComPrefixo()
    {
        var plan = FlowMapper.Materializer.Materializer.BuildPlanFlat<ClienteDto>();

        Assert.Equal(3, plan.Bindings.Count);

        var perfilBinding = plan.Bindings.FirstOrDefault(b => b.ColumnName == "Perfil_Nome");
        Assert.NotNull(perfilBinding);
        Assert.Equal(typeof(string), perfilBinding.PropertyType);
    }
}
