namespace ListaTarefas.Api.Dtos;

public class RespuestaAutenticacionDTO
{
    public string Token { get; set; } = null!; // JWT gerado
    public string RefreshToken { get; set; } = null!; // Refresh token
    public DateTime Expiracion { get; set; } // Expiração do JWT
}
