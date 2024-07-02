namespace PraticaDDD.Domain.Entities.Veiculo
{
    public record CustoVeiculo
    {
        private CustoVeiculo (string codigo )=> Codigo = codigo;
        public string? Codigo { get; init; }

    }
}
