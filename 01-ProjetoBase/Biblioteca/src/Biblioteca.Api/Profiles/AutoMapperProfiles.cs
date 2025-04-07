using AutoMapper;
using Biblioteca.Api.Dtos;
using Biblioteca.Api.Entidades;

namespace Biblioteca.Api.Profiles;

public class AutoMapperProfiles: Profile
{

    public AutoMapperProfiles()
    {
        CreateMap<Autor, AutorReadDto>()
            .ForMember(dto => dto.NomeCompleto,config=> config.MapFrom(autor => $"{autor.Nome} {autor.Apelido}"));
        CreateMap< AutorCreateDto,Autor>();
    }
}