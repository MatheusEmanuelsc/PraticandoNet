using System.ComponentModel.DataAnnotations;

namespace Curso.Api.Dtos
{
    public class DisciplinaDtoUpdate 

    {
        public int Aulas { get; set; }
        public string? Nome { get; set; } 
        public string? Descricao { get; set; }

        
    }

}
