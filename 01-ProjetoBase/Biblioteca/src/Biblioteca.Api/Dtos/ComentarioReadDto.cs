namespace Biblioteca.Api.Dtos;

public class ComentarioReadDto
{
    public Guid Id { get; set; }
    public  string Cuerpo { get; set; }
    public DateTime FechaPublicacion { get; set; }
}