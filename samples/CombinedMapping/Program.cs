using CombinedMapping;
using FlowMapper.Abstractions;
using FlowMapper.DependencyInjection;
using FlowMapper.Providers.SqlServer;
using Microsoft.Extensions.DependencyInjection;

// Configure DI with FlowMapper
var services = new ServiceCollection();

services.AddFlowMapper(builder =>
{
    builder.AddProfile<AppProfile>();
    builder.AddProvider<SqlServerProvider>("Server=localhost;Database=FlowMapperDemo;Trusted_Connection=True;");
});

var sp = services.BuildServiceProvider();

// Initialize Map<TDest>() extension support
DataMapperExtensions.Initialize(sp);

var rapid = sp.GetRequiredService<IRapidMapper>();

// Combined query + map pipeline
var usuarios = await rapid
    .QueryAsync<Usuario>("SELECT Id, Nome, Email FROM Usuarios")
    .Map<Usuario, UsuarioDto>();

foreach (var dto in usuarios)
    Console.WriteLine($"{dto.Id}: {dto.NomeCompleto}");
