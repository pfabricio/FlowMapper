using CombinedMapping;
using FlowMapper.Abstractions;
using FlowMapper.DependencyInjection;
using FlowMapper.Providers.SqlServer;
using Microsoft.Extensions.DependencyInjection;

// ── Config DI ──
var services = new ServiceCollection();

services.AddFlowMapper(builder =>
{
    builder.AddProfile<AppProfile>();
    builder.AddProvider<SqlServerProvider>(
        "Server=localhost;Database=FlowMapperDemo;Trusted_Connection=True;");
});

services.AddTransient<MeuServico>();

var sp = services.BuildServiceProvider();
var app = sp.GetRequiredService<MeuServico>();
await app.Executar();

// ── DTOs ──
public class Usuario
{
    public int Id { get; set; }
    public string Nome { get; set; } = "";
    public string Email { get; set; } = "";
}

public class UsuarioDto
{
    public int Id { get; set; }
    public string NomeCompleto { get; set; } = "";
}

public class ClienteDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = "";
    public PerfilDto Perfil { get; set; } = null!;   // ← nested
}

public class PerfilDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = "";
}

public class ClienteCsvDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = "";
    public int PerfilId { get; set; }
    public string PerfilNome { get; set; } = "";
}

// ── Service ──
public class MeuServico
{
    private readonly IFlowMapper _flow;

    public MeuServico(IFlowMapper flow)
    {
        _flow = flow;
    }

    public async Task Executar()
    {
        // ═══ 1. Objeto → Objeto (AutoMapper-like) ═══
        var usuario = new Usuario { Id = 1, Nome = "Joao", Email = "joao@email.com" };
        var dto = _flow.Map<Usuario, UsuarioDto>(usuario);
        Console.WriteLine($"Map: {dto.Id} - {dto.NomeCompleto}");

        // ═══ 2. SQL → DTO com nested (cascade via alias) ═══
        var clientes = await _flow.QueryAsync<ClienteDto>(@"
            SELECT u.Id, u.Nome,
                   p.Id   AS Perfil_Id,
                   p.Nome AS Perfil_Nome
            FROM Usuario u
            LEFT JOIN Perfil p ON p.UsuarioId = u.Id");
        foreach (var c in clientes)
            Console.WriteLine($"SQL > {c.Id}: {c.Nome} / Perfil: {c.Perfil.Nome}");

        // ═══ 3. JSON → DTO com nested ═══
        var json = """
            {
                "Id": 1,
                "Nome": "Maria",
                "Perfil": { "Id": 10, "Nome": "Admin" }
            }
            """;
        var jDto = _flow.FromJson<ClienteDto>(json);
        Console.WriteLine($"JSON > {jDto.Id}: {jDto.Nome} / Perfil: {jDto.Perfil.Nome}");

        // ═══ 4. JSON array → lista ═══
        var jsonArray = """
            [
                { "Id": 1, "Nome": "Joao" },
                { "Id": 2, "Nome": "Maria" }
            ]
            """;
        var listaJson = _flow.FromJsonList<Usuario>(jsonArray);
        foreach (var u in listaJson)
            Console.WriteLine($"JSON[] > {u.Id}: {u.Nome}");

        // ═══ 5. XML → DTO com nested ═══
        var xml = """
            <ClienteDto>
                <Id>1</Id>
                <Nome>Pedro</Nome>
                <Perfil>
                    <Id>10</Id>
                    <Nome>Suporte</Nome>
                </Perfil>
            </ClienteDto>
            """;
        var xDto = _flow.FromXml<ClienteDto>(xml);
        Console.WriteLine($"XML > {xDto.Id}: {xDto.Nome} / Perfil: {xDto.Perfil.Nome}");

        // ═══ 6. TXT com header (plano, sem nested) ═══
        var csv = new[]
        {
            "Id,Nome,PerfilId,PerfilNome",
            "1,Joao,10,Admin",
            "2,Maria,20,Suporte"
        };
        var listaCsv = _flow.FromText<ClienteCsvDto>(csv, TextDelimiter.PontoVirgula, hasHeader: true);
        foreach (var c in listaCsv)
            Console.WriteLine($"TXT > {c.Id}: {c.Nome} / PerfilId={c.PerfilId}");

        // ═══ 7. TXT posicional (sem header) ═══
        var posicional = new[] { "1,Joao", "2,Maria" };
        var listaPos = _flow.FromText<Usuario>(posicional, TextDelimiter.PontoVirgula, hasHeader: false);
        foreach (var u in listaPos)
            Console.WriteLine($"TXT(pos) > {u.Id}: {u.Nome}");
    }
}
