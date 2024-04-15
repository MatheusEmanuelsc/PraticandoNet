using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using _03_DtoAutoMapper.Domain.Dto;
using _03_DtoAutoMapper.Domain.Model;
using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;

namespace _03_DtoAutoMapper.Domain.Profiles
{
    public class UserProfile:Profile
    {
        public UserProfile()
        {
            CreateMap<User, UserResponseDto>();
            CreateMap<UserRequestDto, User>();
        }
    }
}