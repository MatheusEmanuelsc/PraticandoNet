using System.ComponentModel.DataAnnotations;

namespace ListaTarefas.Api.Dtos;

public class ResetPasswordDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = null!; // E-mail
    [Required]
    public string Token { get; set; } = null!; // Token de redefinição
    [Required, MinLength(8)]
    public string NovaSenha { get; set; } = null!; // Nova senha
}