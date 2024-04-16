using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace _04_Database.Domain.Model
{
    public class Atendimento
    {
        [Key]
        [Required]  
        public int Id { get; set; }
        public bool EstaAtivo { get; set; } = true;

        [Required]    
        public string Nome { get; set; } =null!;
        public string Relato { get; set; } =string.Empty;
        public bool EstaResolvido { get; set; }
        public bool DesejaRetorno { get; set; }

        public DateTime? CriadoEm { get; set; } = DateTime.Now;
        public DateTime? AtualizadoEm { get; set; }
        public DateTime? DeletadoEm { get; set; }
    }
}