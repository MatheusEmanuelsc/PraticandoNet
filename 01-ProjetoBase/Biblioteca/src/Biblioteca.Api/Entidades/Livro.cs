using System.ComponentModel.DataAnnotations;

namespace Biblioteca.Api.Entidades;

public class Livro
{
    public int Id { get; set; }
    [Required]
    public required string  Nome { get; set; }
    public int AutorId { get; set; }
    public Autor? Autor { get; set; }

    public List<Comentario> Comentarios { get; set; } = [];
}