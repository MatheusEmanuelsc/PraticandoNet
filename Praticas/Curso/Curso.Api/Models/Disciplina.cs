using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Curso.Api.Models
{
    public class Disciplina
    {
        [Key]
        public int DisciplinaId { get; set; }
        public int Aulas { get; set; }
        public string Nome { get; set; }=string.Empty;
        public string? Descricao { get; set; }

        [JsonIgnore]
        public ICollection<Aluno>? Alunos { get; set; }= new List<Aluno>(); 
    }
}
