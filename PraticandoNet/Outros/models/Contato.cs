using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Outros.models
{
    public class Contato
    {
        public int ContatoId { get; set; }
        public string Nome { get; set; } = "";
        public string Telefone { get; set; } = "";
        public bool Ativo { get; set; }
    }
}