using System.ComponentModel.DataAnnotations;

namespace ListaTarefas.Api.Dtos;

public class RefreshTokenDTO
{
    [Required]
    public string RefreshToken { get; set; } = null!; // Refresh token
}