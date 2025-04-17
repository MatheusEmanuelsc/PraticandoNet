namespace ListaTarefas.Api.Entities;

public class RefreshToken
{
    public int Id { get; set; } // Identificador único
    public string Token { get; set; } = null!; // Valor do refresh token
    public string UserId { get; set; } = null!; // ID do usuário
    public string JwtId { get; set; } = null!; // ID do JWT associado
    public DateTime AddedDate { get; set; } // Data de criação
    public DateTime Expiration { get; set; } // Data de expiração
    public bool IsUsed { get; set; } // Indica se foi usado
    public bool IsRevoked { get; set; } // Indica se foi revogado
}