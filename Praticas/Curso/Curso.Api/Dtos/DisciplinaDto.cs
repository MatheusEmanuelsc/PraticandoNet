using System.ComponentModel.DataAnnotations;


namespace Curso.Api.Dtos
{
    public class DisciplinaDto
    {
       
        public int DisciplinaId { get; set; }
        public int Aulas { get; set; }
        [Required]
        public string Nome { get; set; } = string.Empty;
        public string? Descricao { get; set; }
    }
}
