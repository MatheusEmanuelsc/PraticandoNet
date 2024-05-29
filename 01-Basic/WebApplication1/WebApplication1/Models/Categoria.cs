

using System.Collections.ObjectModel;

namespace WebApplication1.Models;

    public class Categoria
    {
    public int CategoriaId { get; set; }
    public string? Nome { get; set; }
    public string? ImgUrl { get; set; }

    public Categoria()
    {
        // E uma boa pratica que a prpria classe inicie a coleção
        Produtos = new Collection<Produto>();
    }

    // Define o Relacionamento 1 para muitos
    public ICollection<Produto>? Produtos { get; set; }  
}

