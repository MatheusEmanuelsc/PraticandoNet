namespace BibliotecaAPI.Entidades;

public class RefreshToken
{
    public int Id { get; set; }
    public string Token { get; set; } = null!;
    public string UserId { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
    public DateTime Expiration { get; set; }
    public bool IsRevoked { get; set; }
}