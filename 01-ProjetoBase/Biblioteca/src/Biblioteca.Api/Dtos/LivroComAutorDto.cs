namespace Biblioteca.Api.Dtos;

public class LivroComAutorDto:LivroReadDto
{
    public int AutorId { get; set; }
    public required string AutorNome { get; set; }
}