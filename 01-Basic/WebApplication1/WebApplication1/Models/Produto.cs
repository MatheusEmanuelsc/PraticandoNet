namespace WebApplication1.Models;

    public class Produto
    {
    public int ProdutoId { get; set; }
    public string? Nome { get; set; }
    public string? Desc { get; set; }
    public decimal Preco { get; set; }
    public string? ImgUrl { get; set; }
    public double Estoque { get; set; }
    public DateTime DataCadastro { get; set; }

    // E opcional mais vc pode explicita ainda mais o Relacionamento

    public int CategoriaId { get; set; }
    public Categoria? Categoria { get; set; }
}

