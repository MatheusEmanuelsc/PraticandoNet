using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using _02_DtoAutoMapper.Models;
namespace _02_DtoAutoMapper.Dtos
{
    public class UserDto
    {
        public int Id { get; set; }
        public string? Email { get; set; }
        public string? UserName { get; set; }

       
        public UserDto(User user)
        {
            Id =user.Id;
            Email =user.Email;
            UserName =user.UserName;
        }
    }
}