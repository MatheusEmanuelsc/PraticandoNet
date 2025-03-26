using System.ComponentModel.DataAnnotations;

namespace Biblioteca.Api.Entidades;

public class Autor
{
    public int Id { get; set; }
    [Required]
    public required string  Nome { get; set; }
}