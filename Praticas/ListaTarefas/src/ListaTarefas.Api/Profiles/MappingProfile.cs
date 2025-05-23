using AutoMapper;
using ListaTarefas.Api.Dtos;
using ListaTarefas.Api.Dtos.Tarefas;
using ListaTarefas.Api.Entities;

namespace ListaTarefas.Api.Profiles;

public class MappingProfile: Profile
{
    public MappingProfile()
    {
        CreateMap<Tarefa, TarefaCreateDto>().ReverseMap();
        CreateMap<Tarefa, TarefaReadDto>().ReverseMap();
        CreateMap<Tarefa, TarefaUpdateDto>().ReverseMap();
    }
}