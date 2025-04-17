using System.ComponentModel.DataAnnotations;

namespace ListaTarefas.Api.Dtos;

public class LoginDTO
{
    [Required]
    public string UserName { get; set; } = null!; // Nome de usuário
    [Required]
    public string Password { get; set; } = null!; // Senha
}