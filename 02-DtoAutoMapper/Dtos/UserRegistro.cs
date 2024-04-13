using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;


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
        [StringLength(8)]
        public string? Password { get; set; }
    }  
}