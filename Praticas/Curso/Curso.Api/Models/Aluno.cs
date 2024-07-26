using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Curso.Api.Models
{
    public class Aluno
    {
        [Key]
        public int AlunoId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public double Nota { get; set; }

        
        public int DisciplinaId { get; set; }

        [JsonIgnore]
        public Disciplina? Disciplina { get; set; }

    }
}
