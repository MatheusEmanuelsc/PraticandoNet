using AutoMapper;
using Biblioteca.Api.Dtos;
using Biblioteca.Api.Entidades;

namespace Biblioteca.Api.Profiles;

public class AutoMapperProfiles: Profile
{

    public AutoMapperProfiles()
    {
        CreateMap<Autor, AutorReadDto>()
            .ForMember(dto => dto.NomeCompleto,
                config=> config.MapFrom(autor => MapearNombreYApellidoAutor(autor)));
        CreateMap<Autor, AutorComLivrosDto>()
            .ForMember(dto => dto.NomeCompleto,
                config=> config.MapFrom(autor => MapearNombreYApellidoAutor(autor)));
        CreateMap< AutorCreateDto,Autor>();
        
        CreateMap<Livro, LivroReadDto>();
        CreateMap<LivroCreateDto, Livro>();
        CreateMap<Livro, LivroComAutorDto>()
            .ForMember(dto => dto.AutorNome,
                config=> config.MapFrom(autor => MapearNombreYApellidoAutor(autor.Autor!)));
    }
    private string MapearNombreYApellidoAutor(Autor autor) => $"{autor.Nome} {autor.Apelido}";
}