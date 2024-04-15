using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using _03_DtoAutoMapper.Domain.Dto;
using _03_DtoAutoMapper.Domain.Model;

namespace _03_DtoAutoMapper.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IMapper _mapper;

        public UserController(IMapper mapper)
        {
            _mapper = mapper;
        }

        private static List<User> _users = new(){
            new(){
                Id = 1,
                UserName="jao",
                Email = "jao@seilar.com",
                Password =  "1234"

            },
            new(){
                Id = 2,
                UserName="bao",
                Email = "bao@seilar.com",
                Password =  "5678"

            }
        };
        [HttpGet]
        [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
        public IActionResult GetAll()
        {

            if (_users.Count == 0)
            {
                return NoContent();
            }

            // var userDtos = _users.Select(user => new UserDto
            // {
            //     Id = user.Id,
            //     UserName = user.UserName,
            //     Email = user.Email
            // });

            
            var userResponse = _users.Select(user =>_mapper.Map<UserResponseDto>(user));
            // uma forma de repetir menos codigo simplemnte criar o construtor na classe  passando os dados
            // var userResponse = _users.Select(_mapper.Map<UserResponseDto>);
            return Ok(userResponse);
        }

        [HttpPost]
        public IActionResult Registrar(UserRequestDto userRegistro)
        {
            bool isEmailAlreadyUser = _users.Any(user => user.Email == userRegistro.Email);


            if (isEmailAlreadyUser)
            {
                return BadRequest();
            }

            // User user = new User(){
            //     Id = _users.Count +1,
            //     UserName = userRegistro.UserName,
            //     Email = userRegistro.Email,
            //     Password = userRegistro.Password
            // };
            User user = _mapper.Map<User>(userRegistro);
            user.Id = _users.Count + 1;
            _users.Add(user);
            return Created();
        }
    }
}