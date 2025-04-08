using System.ComponentModel.DataAnnotations;

namespace Biblioteca.Api.Dtos;

public class AutorPatchDto
{
    [Required(ErrorMessage = "O campo {0} é obrigatório")]
    [StringLength(150, ErrorMessage = "O campo {0} deve ter {1} caracteres ou menos")]
    public required string Nome { get; set; }

    [Required(ErrorMessage = "O campo {0} é obrigatório")]
    [StringLength(150, ErrorMessage = "O campo {0} deve ter {1} caracteres ou menos")]
    public required string Apelido { get; set; }
    
    [StringLength(20, ErrorMessage = "O campo {0} deve ter {1} caracteres ou menos")]
    public string? Identificacao { get; set; }
}