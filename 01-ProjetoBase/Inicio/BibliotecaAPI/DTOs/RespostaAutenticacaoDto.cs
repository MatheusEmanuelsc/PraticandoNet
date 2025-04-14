namespace BibliotecaAPI.DTOs;

public class RespostaAutenticacaoDto
{
    public string Token { get; set; }
    public DateTime Expiracao { get; set; }
}