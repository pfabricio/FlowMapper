using BasicMapping;
using FlowMapper.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddFlowMapper(builder =>
{
    builder.AddProfile<AppProfile>();
});

var provider = services.BuildServiceProvider();
var mapper = provider.GetRequiredService<FlowMapper.Abstractions.IFlowMapper>();

var usuario = new Usuario { Id = 1, Nome = "João", Email = "joao@email.com" };
var dto = mapper.Map<Usuario, UsuarioDto>(usuario);

Console.WriteLine($"Id: {dto.Id}, NomeCompleto: {dto.NomeCompleto}");
