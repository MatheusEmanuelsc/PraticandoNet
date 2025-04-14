using System.ComponentModel.DataAnnotations;

namespace BibliotecaAPI.DTOs;

public class CredencialUserDto
{
    [Required]
    public string Email { get; set; }
    [Required]
    public string Password { get; set; }
}