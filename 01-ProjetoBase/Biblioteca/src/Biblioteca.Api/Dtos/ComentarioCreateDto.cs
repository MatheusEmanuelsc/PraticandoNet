using System.ComponentModel.DataAnnotations;

namespace Biblioteca.Api.Dtos;

public class ComentarioCreateDto
{
    [Required]
    public required string Cuerpo { get; set; }
}