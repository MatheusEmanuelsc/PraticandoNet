using Curso.Api.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Curso.Api.Dtos
{
    public class AlunoDto
    {
        
        public int AlunoId { get; set; }
        [Required]
        public string Nome { get; set; } = string.Empty;
        public double Nota { get; set; }
        public int? DisciplinaId { get; set; }
    }
}
