using PraticaDDD.Domain.Entities.Abstractions;

namespace PraticaDDD.Domain.Entities.Veiculo
{
    public sealed class Veiculo : Entity
    {
        public Veiculo(Guid id) : base(id)
        {
        }

       
        public Modelo Modelo { get; private set; }
        public CodigoVeiculo CodigoVeiculo { get; private set; }

        public Endereco? Endereco { get; private set; }
       

        public decimal Preco { get; private set; }
        public string? TipoMoeda { get; private set; }
        public decimal PrecoManutencao { get; private set; }

        public List<Acessorios> Acessorios { get; private set; } = new();

    }


    
}
