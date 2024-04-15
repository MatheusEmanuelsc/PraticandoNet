using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using _02_DtoAutoMapper.Models;


namespace _02_DtoAutoMapper.Dtos
{
    public class UserRegistro
    {
        [Required]
        [EmailAddress]
        public string? Email { get; set; }
        [Required]
        public string? UserName { get; set; }
        [Required]

        public string? Password { get; set; }


        public User ToUser()
        {
            return new User() { Email = Email, UserName = UserName, Password = Password };
        }
    }
}