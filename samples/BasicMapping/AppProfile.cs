using FlowMapper.Core;

namespace BasicMapping;

public class AppProfile : ProfileDefinition
{
    public AppProfile()
    {
        ProfileName = "AppProfile";

        CreateMap<Usuario, UsuarioDto>()
            .ForMember(d => d.NomeCompleto, opt => opt.MapFrom(s => $"{s.Nome} ({s.Email})"));
    }
}

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
