﻿using AutoMapper;
using Curso.Api.Dtos;
using Curso.Api.Models;

namespace Curso.Api.Profiles
{
    public class MappingProfile:Profile
    {
        public MappingProfile()
        {
            CreateMap<Aluno, AlunoDto>().ReverseMap();
            CreateMap<Aluno, AlunoDtoUpdate>().ReverseMap();

            CreateMap<Disciplina, DisciplinaDto>().ReverseMap();
            CreateMap<Disciplina, DisciplinaDtoUpdate>().ReverseMap();
        }
    }
}
