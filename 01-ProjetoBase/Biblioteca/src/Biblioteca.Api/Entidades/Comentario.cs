using System.ComponentModel.DataAnnotations;

namespace Biblioteca.Api.Entidades;

public class Comentario
{
    public Guid Id { get; set; }
    [Required]
    public required string Cuerpo { get; set; }
    public DateTime FechaPublicacion { get; set; }
    public int LivroId { get; set; }
    public Livro? Livro { get; set; }
}