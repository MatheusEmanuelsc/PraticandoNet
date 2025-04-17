using System.ComponentModel.DataAnnotations;

namespace ListaTarefas.Api.Dtos;

public class RegisterDto
{
    [Required]
    public string UserName { get; set; } = null!; // Nome de usuário
    [Required, EmailAddress]
    public string Email { get; set; } = null!; // E-mail válido
    [Required, MinLength(8)]
    public string Password { get; set; } = null!; // Senha com mínimo de 8 caracteres
    [Required]
    public string NomeCompleto { get; set; } = null!; // Nome completo
}