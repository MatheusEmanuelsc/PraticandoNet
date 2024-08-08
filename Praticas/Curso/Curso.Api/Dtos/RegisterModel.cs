using System.ComponentModel.DataAnnotations;

namespace Curso.Api.Dtos
{
    public class RegisterModel
    {
        [Required(ErrorMessage = "User Name is required")]
        public string?  Username { get; set; }
        [Required(ErrorMessage = "User Name is required")]
        public string? Email { get; set; }
        [Required(ErrorMessage = "User Name is required")]
        public string? Password { get; set; }
    }
}
