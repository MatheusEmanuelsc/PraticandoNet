using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using _02_DtoAutoMapper.Models;
using _02_DtoAutoMapper.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;

namespace _02_DtoAutoMapper.Controllers
{
    [ApiController]
    [Route("users")]
    public class UserController : ControllerBase
    { 

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
        public IActionResult GetAll(){

            if (_users.Count == 0){
                return NoContent();
            }

            // var userDtos = _users.Select(user => new UserDto
            // {
            //     Id = user.Id,
            //     UserName = user.UserName,
            //     Email = user.Email
            // });

            // uma forma de repetir menos codigo simplemnte criar o construtor na classe  passando os dados
            var userResponse= _users.Select(user=>new UserDto(user));
            return Ok(userResponse);
        }

        [HttpPost]
        public IActionResult Registrar(UserRegistro userRegistro){
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
            User user = userRegistro.ToUser();
            user.Id = _users.Count + 1;
            _users.Add(user);
            return Created();
        }
    }
}
