using Curso.Api.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Curso.Api.Dtos
{
    public class AlunoDto
    {
        [Required]
        public string Nome { get; set; } = string.Empty;
        public double Nota { get; set; }
        public DisciplinaDto? Disciplina { get; set; }
    }
}
